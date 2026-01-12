using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;
using DotnetApiDemo.BackgroundServices;
using DotnetApiDemo.Data;
using DotnetApiDemo.Data.Seeds;
using DotnetApiDemo.Middleware;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Services.Implementations;
using DotnetApiDemo.Services.Interfaces;

// 配置 Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("啟動應用程式...");

    var builder = WebApplication.CreateBuilder(args);

    // 使用 Serilog
    builder.Host.UseSerilog();

    // 配置服務
    ConfigureServices(builder.Services, builder.Configuration);

    var app = builder.Build();

    // 配置中介軟體
    ConfigureMiddleware(app);

    // 執行資料庫遷移和種子資料
    await InitializeDatabaseAsync(app);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "應用程式啟動失敗");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// 配置服務
/// </summary>
void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // 配置 DbContext
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

    // 配置 Identity
    services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        // 密碼設定
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;

        // 帳戶設定
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = false;

        // 鎖定設定
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

    // 配置 JWT
    var jwtSettings = configuration.GetSection("Jwt");
    var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

    // 配置 Redis 快取
    var redisConnectionString = configuration.GetConnectionString("Redis");
    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "DotnetApiDemo:";
    });

    // 註冊 Redis ConnectionMultiplexer（用於進階操作）
    services.AddSingleton<IConnectionMultiplexer>(sp =>
        ConnectionMultiplexer.Connect(redisConnectionString!));

    // 註冊快取服務
    services.AddSingleton<ICacheService, RedisCacheService>();

    // 註冊 Token 黑名單服務
    services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();

    // 註冊使用者在線狀態服務
    services.AddSingleton<IUserPresenceService, UserPresenceService>();

    // 註冊速率限制服務
    services.AddSingleton<IRateLimitService, RateLimitService>();

    // 註冊分散式鎖服務
    services.AddSingleton<IDistributedLockService, DistributedLockService>();

    // 註冊通知服務
    services.AddScoped<INotificationService, NotificationService>();

    // 註冊審計日誌佇列服務
    services.AddSingleton<IAuditQueueService, AuditQueueService>();

    // 註冊背景服務
    services.AddHostedService<AuditLogProcessorService>();
    services.AddHostedService<SuspendedOrderCleanupService>();
    services.AddHostedService<ScheduledReportProcessorService>();

    // 配置 SignalR
    services.AddSignalR()
        .AddStackExchangeRedis(redisConnectionString!, options =>
        {
            options.Configuration.ChannelPrefix = RedisChannel.Literal("DotnetApiDemo:");
        });

    // 配置 CORS
    services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // 註冊服務
    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<IRoleService, RoleService>();
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<ICategoryService, CategoryService>();
    services.AddScoped<IProductService, ProductService>();
    services.AddScoped<IStoreService, StoreService>();
    services.AddScoped<IWarehouseService, WarehouseService>();
    services.AddScoped<ICustomerService, CustomerService>();
    services.AddScoped<ICustomerLevelService, CustomerLevelService>();
    services.AddScoped<ISupplierService, SupplierService>();
    services.AddScoped<IInventoryService, InventoryService>();
    services.AddScoped<IStockTransferService, StockTransferService>();
    services.AddScoped<IOrderService, OrderService>();
    services.AddScoped<IPromotionService, PromotionService>();
    services.AddScoped<ICouponService, CouponService>();
    services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
    services.AddScoped<IPurchaseReceiptService, PurchaseReceiptService>();
    services.AddScoped<IPurchaseReturnService, PurchaseReturnService>();
    services.AddScoped<IStockCountService, StockCountService>();
    services.AddScoped<ISupplierPriceService, SupplierPriceService>();
    services.AddScoped<IReportService, ReportService>();
    services.AddScoped<IPaymentMethodService, PaymentMethodService>();
    services.AddScoped<ISalesReturnService, SalesReturnService>();
    services.AddScoped<ISystemSettingService, SystemSettingService>();
    services.AddScoped<IAuditLogService, AuditLogService>();
    services.AddScoped<INumberRuleService, NumberRuleService>();
    services.AddScoped<IProductVariantService, ProductVariantService>();
    services.AddScoped<ICashierShiftService, CashierShiftService>();
    services.AddScoped<IInvoiceService, InvoiceService>();
    services.AddScoped<IPointService, PointService>();
    services.AddScoped<IProductComboService, ProductComboService>();
    services.AddScoped<ILabelService, LabelService>();
    services.AddScoped<ICustomReportService, CustomReportService>();
    services.AddScoped<IUnitService, UnitService>();
    services.AddScoped<ISuspendedOrderService, SuspendedOrderService>();
    services.AddScoped<IPurchaseSuggestionService, PurchaseSuggestionService>();

    // 配置 Controllers
    services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

    // 配置 Swagger
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "龜三的ASP.NET Core Web API Demo",
            Version = "v1",
            Description = "ASP.NET Core Web API 零售業簡易 ERP 系統",
            Contact = new OpenApiContact
            {
                Name = "Demo Developer",
                Email = "demo@example.com"
            }
        });

        // 配置 JWT 驗證
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT 驗證。請在下方輸入 'Bearer {token}'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        // 使用 XML 註解
        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    // 健康檢查
    services.AddHealthChecks()
        .AddMySql(connectionString!, name: "database")
        .AddRedis(configuration.GetConnectionString("Redis")!, name: "redis");
}

/// <summary>
/// 配置中介軟體
/// </summary>
void ConfigureMiddleware(WebApplication app)
{
    // 全域例外處理
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "伺服器發生錯誤",
                errors = new[] { "請稍後再試或聯繫系統管理員" }
            });
        });
    });

    // Swagger (開發環境)
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "龜三的ASP.NET Core Web API Demo v1");
            options.RoutePrefix = string.Empty;
        });
    }

    // HTTPS 重導向 (生產環境)
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    // CORS
    app.UseCors("AllowAll");

    // API 速率限制
    app.UseRateLimit();

    // Serilog 請求日誌
    app.UseSerilogRequestLogging();

    // 驗證與授權
    app.UseAuthentication();
    app.UseTokenValidation(); // Token 黑名單驗證
    app.UseAuthorization();

    // 健康檢查端點
    app.MapHealthChecks("/health");

    // 控制器路由
    app.MapControllers();

    // SignalR Hub 路由
    app.MapHub<DotnetApiDemo.Hubs.NotificationHub>("/hubs/notifications");
}

/// <summary>
/// 初始化資料庫
/// </summary>
async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // 確保資料庫已建立（適用於開發環境）
        Log.Information("確保資料庫結構已建立...");
        await context.Database.EnsureCreatedAsync();

        // 執行種子資料
        Log.Information("執行種子資料...");
        await DataSeeder.SeedAsync(services);

        Log.Information("資料庫初始化完成");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "資料庫初始化失敗");
        throw;
    }
}
