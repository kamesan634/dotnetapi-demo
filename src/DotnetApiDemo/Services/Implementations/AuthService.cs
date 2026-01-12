using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Auth;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

/// <summary>
/// 驗證服務實作
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly IDistributedCache _cache;
    private readonly ITokenBlacklistService _tokenBlacklistService;
    private readonly IUserPresenceService _userPresenceService;
    private readonly ILogger<AuthService> _logger;

    /// <summary>
    /// 建構函式
    /// </summary>
    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        IDistributedCache cache,
        ITokenBlacklistService tokenBlacklistService,
        IUserPresenceService userPresenceService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _cache = cache;
        _tokenBlacklistService = tokenBlacklistService;
        _userPresenceService = userPresenceService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TokenResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("登入失敗：找不到使用者或使用者已停用 - {UserName}", request.UserName);
            return null;
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            _logger.LogWarning("登入失敗：密碼錯誤 - {UserName}", request.UserName);
            return null;
        }

        // 更新最後登入時間
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var token = await GenerateTokenResponseAsync(user, roles, request.RememberMe);

        // 設定使用者上線狀態
        await _userPresenceService.SetOnlineAsync(user.Id);

        _logger.LogInformation("使用者登入成功 - {UserName}", request.UserName);
        return token;
    }

    /// <inheritdoc />
    public async Task<(bool Success, IEnumerable<string> Errors)> RegisterAsync(RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            RealName = request.FullName,
            PhoneNumber = request.Phone,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            _logger.LogWarning("使用者註冊失敗 - {UserName}: {Errors}",
                request.UserName,
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return (false, result.Errors.Select(e => e.Description));
        }

        // 預設加入 Staff 角色
        await _userManager.AddToRoleAsync(user, "Staff");

        _logger.LogInformation("使用者註冊成功 - {UserName}", request.UserName);
        return (true, Enumerable.Empty<string>());
    }

    /// <inheritdoc />
    public async Task<TokenResponse?> RefreshTokenAsync(RefreshTokenRequest request)
    {
        // 從快取取得 Refresh Token 資訊
        var userId = await _cache.GetStringAsync($"refresh_token:{request.RefreshToken}");
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("刷新 Token 失敗：Token 無效或已過期");
            return null;
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("刷新 Token 失敗：使用者不存在或已停用");
            return null;
        }

        // 移除舊的 Refresh Token
        await _cache.RemoveAsync($"refresh_token:{request.RefreshToken}");

        var roles = await _userManager.GetRolesAsync(user);
        var token = await GenerateTokenResponseAsync(user, roles, false);

        _logger.LogInformation("Token 刷新成功 - UserId: {UserId}", userId);
        return token;
    }

    /// <inheritdoc />
    public async Task<bool> LogoutAsync(int userId)
    {
        try
        {
            // 將使用者所有 Token 加入黑名單
            await _tokenBlacklistService.BlacklistUserTokensAsync(userId);

            // 設定使用者離線
            await _userPresenceService.SetOfflineAsync(userId);

            _logger.LogInformation("使用者登出成功 - UserId: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "使用者登出失敗 - UserId: {UserId}", userId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<(bool Success, IEnumerable<string> Errors)> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return (false, new[] { "找不到使用者" });
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            _logger.LogWarning("修改密碼失敗 - UserId: {UserId}: {Errors}",
                userId,
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return (false, result.Errors.Select(e => e.Description));
        }

        _logger.LogInformation("修改密碼成功 - UserId: {UserId}", userId);
        return (true, Enumerable.Empty<string>());
    }

    /// <inheritdoc />
    public async Task<UserInfo?> GetCurrentUserAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new UserInfo
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            FullName = user.RealName ?? user.UserName!,
            Roles = roles
        };
    }

    /// <summary>
    /// 產生 Token 回應
    /// </summary>
    private async Task<TokenResponse> GenerateTokenResponseAsync(ApplicationUser user, IList<string> roles, bool rememberMe)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jti = Guid.NewGuid().ToString();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email!),
            new("FullName", user.RealName ?? user.UserName!),
            new(JwtRegisteredClaimNames.Jti, jti)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var expiresInMinutes = rememberMe ? 7 * 24 * 60 : int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");
        var expires = DateTime.UtcNow.AddMinutes(expiresInMinutes);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        // 追蹤使用者的 Token（用於登出時加入黑名單）
        await _tokenBlacklistService.TrackUserTokenAsync(user.Id, jti, expires);

        // 產生 Refresh Token
        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiry = rememberMe ? TimeSpan.FromDays(30) : TimeSpan.FromDays(7);

        // 儲存 Refresh Token 到快取
        await _cache.SetStringAsync(
            $"refresh_token:{refreshToken}",
            user.Id.ToString(),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = refreshTokenExpiry
            });

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = expiresInMinutes * 60,
            User = new UserInfo
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                FullName = user.RealName ?? user.UserName!,
                Roles = roles
            }
        };
    }

    /// <summary>
    /// 產生 Refresh Token
    /// </summary>
    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
