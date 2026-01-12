using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Labels;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Services.Interfaces;

namespace DotnetApiDemo.Services.Implementations;

public class LabelService : ILabelService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LabelService> _logger;

    public LabelService(ApplicationDbContext context, ILogger<LabelService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Label Templates

    public async Task<PaginatedResponse<LabelTemplateListDto>> GetTemplatesAsync(PaginationRequest request, string? type = null)
    {
        var query = _context.LabelTemplates.AsQueryable();

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(t => t.Type == type);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(t => t.Name.Contains(request.Search) || t.Code.Contains(request.Search));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(t => t.Code)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new LabelTemplateListDto
            {
                Id = t.Id,
                Code = t.Code,
                Name = t.Name,
                Type = t.Type,
                Width = t.Width,
                Height = t.Height,
                IsActive = t.IsActive,
                IsDefault = t.IsDefault
            })
            .ToListAsync();

        return new PaginatedResponse<LabelTemplateListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<LabelTemplateDetailDto?> GetTemplateByIdAsync(int id)
    {
        return await _context.LabelTemplates
            .Where(t => t.Id == id)
            .Select(t => new LabelTemplateDetailDto
            {
                Id = t.Id,
                Code = t.Code,
                Name = t.Name,
                Type = t.Type,
                Width = t.Width,
                Height = t.Height,
                LayoutJson = t.LayoutJson,
                IsActive = t.IsActive,
                IsDefault = t.IsDefault,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<LabelTemplateListDto>> GetActiveTemplatesAsync(string? type = null)
    {
        var query = _context.LabelTemplates.Where(t => t.IsActive);

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(t => t.Type == type);

        return await query
            .OrderByDescending(t => t.IsDefault)
            .ThenBy(t => t.Name)
            .Select(t => new LabelTemplateListDto
            {
                Id = t.Id,
                Code = t.Code,
                Name = t.Name,
                Type = t.Type,
                Width = t.Width,
                Height = t.Height,
                IsActive = t.IsActive,
                IsDefault = t.IsDefault
            })
            .ToListAsync();
    }

    public async Task<int?> CreateTemplateAsync(CreateLabelTemplateRequest request)
    {
        if (await _context.LabelTemplates.AnyAsync(t => t.Code == request.Code))
        {
            _logger.LogWarning("建立標籤模板失敗：代碼已存在 - {Code}", request.Code);
            return null;
        }

        // If setting as default, unset other defaults of same type
        if (request.IsDefault)
        {
            var existingDefaults = await _context.LabelTemplates
                .Where(t => t.Type == request.Type && t.IsDefault)
                .ToListAsync();
            foreach (var t in existingDefaults)
                t.IsDefault = false;
        }

        var template = new LabelTemplate
        {
            Code = request.Code,
            Name = request.Name,
            Type = request.Type,
            Width = request.Width,
            Height = request.Height,
            LayoutJson = request.LayoutJson,
            IsDefault = request.IsDefault,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.LabelTemplates.Add(template);
        await _context.SaveChangesAsync();

        _logger.LogInformation("建立標籤模板成功 - {Code}: {Name}", template.Code, template.Name);
        return template.Id;
    }

    public async Task<bool> UpdateTemplateAsync(int id, UpdateLabelTemplateRequest request)
    {
        var template = await _context.LabelTemplates.FindAsync(id);
        if (template == null) return false;

        if (!string.IsNullOrWhiteSpace(request.Name))
            template.Name = request.Name;

        if (!string.IsNullOrWhiteSpace(request.Type))
            template.Type = request.Type;

        if (request.Width.HasValue)
            template.Width = request.Width.Value;

        if (request.Height.HasValue)
            template.Height = request.Height.Value;

        if (request.LayoutJson != null)
            template.LayoutJson = request.LayoutJson;

        if (request.IsActive.HasValue)
            template.IsActive = request.IsActive.Value;

        if (request.IsDefault == true)
        {
            var existingDefaults = await _context.LabelTemplates
                .Where(t => t.Type == template.Type && t.IsDefault && t.Id != id)
                .ToListAsync();
            foreach (var t in existingDefaults)
                t.IsDefault = false;
            template.IsDefault = true;
        }
        else if (request.IsDefault == false)
        {
            template.IsDefault = false;
        }

        template.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("更新標籤模板成功 - Id: {Id}", id);
        return true;
    }

    public async Task<bool> DeleteTemplateAsync(int id)
    {
        var template = await _context.LabelTemplates.FindAsync(id);
        if (template == null) return false;

        _context.LabelTemplates.Remove(template);
        await _context.SaveChangesAsync();

        _logger.LogInformation("刪除標籤模板成功 - Id: {Id}", id);
        return true;
    }

    #endregion

    #region Print Jobs

    public async Task<PaginatedResponse<PrintJobListDto>> GetPrintJobsAsync(PaginationRequest request, string? status = null)
    {
        var query = _context.PrintJobs
            .Include(j => j.Template)
            .Include(j => j.CreatedBy)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(j => j.Status == status);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(j => new PrintJobListDto
            {
                Id = j.Id,
                JobNo = j.JobNo,
                TemplateName = j.Template.Name,
                Status = j.Status,
                TotalLabels = j.TotalLabels,
                PrintedLabels = j.PrintedLabels,
                PrinterName = j.PrinterName,
                CreatedAt = j.CreatedAt,
                CompletedAt = j.CompletedAt,
                CreatedByName = j.CreatedBy.RealName ?? j.CreatedBy.UserName
            })
            .ToListAsync();

        return new PaginatedResponse<PrintJobListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<PrintJobDetailDto?> GetPrintJobByIdAsync(int id)
    {
        return await _context.PrintJobs
            .Where(j => j.Id == id)
            .Select(j => new PrintJobDetailDto
            {
                Id = j.Id,
                JobNo = j.JobNo,
                TemplateName = j.Template.Name,
                Status = j.Status,
                TotalLabels = j.TotalLabels,
                PrintedLabels = j.PrintedLabels,
                PrinterName = j.PrinterName,
                ErrorMessage = j.ErrorMessage,
                CreatedAt = j.CreatedAt,
                StartedAt = j.StartedAt,
                CompletedAt = j.CompletedAt,
                CreatedByName = j.CreatedBy.RealName ?? j.CreatedBy.UserName,
                Items = j.Items.Select(i => new PrintJobItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductSku = i.Product.Sku,
                    ProductName = i.Product.Name,
                    ProductPrice = i.Product.SellingPrice,
                    Barcode = i.Product.Barcodes.Where(b => b.IsPrimary).Select(b => b.Barcode).FirstOrDefault(),
                    Quantity = i.Quantity,
                    CustomText = i.CustomText
                })
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int?> CreatePrintJobAsync(CreatePrintJobRequest request, int userId)
    {
        var template = await _context.LabelTemplates.FindAsync(request.TemplateId);
        if (template == null || !template.IsActive)
        {
            _logger.LogWarning("建立列印任務失敗：模板不存在或未啟用");
            return null;
        }

        var jobNo = await GenerateJobNoAsync();
        var totalLabels = request.Items.Sum(i => i.Quantity);

        var job = new PrintJob
        {
            JobNo = jobNo,
            TemplateId = request.TemplateId,
            PrinterName = request.PrinterName,
            TotalLabels = totalLabels,
            Status = "Pending",
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in request.Items)
        {
            job.Items.Add(new PrintJobItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                CustomText = item.CustomText
            });
        }

        _context.PrintJobs.Add(job);
        await _context.SaveChangesAsync();

        _logger.LogInformation("建立列印任務成功 - {JobNo}", jobNo);
        return job.Id;
    }

    public async Task<bool> StartPrintJobAsync(int id)
    {
        var job = await _context.PrintJobs.FindAsync(id);
        if (job == null || job.Status != "Pending") return false;

        job.Status = "Processing";
        job.StartedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CompletePrintJobAsync(int id, int printedCount)
    {
        var job = await _context.PrintJobs.FindAsync(id);
        if (job == null) return false;

        job.Status = "Completed";
        job.PrintedLabels = printedCount;
        job.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> FailPrintJobAsync(int id, string errorMessage)
    {
        var job = await _context.PrintJobs.FindAsync(id);
        if (job == null) return false;

        job.Status = "Failed";
        job.ErrorMessage = errorMessage;
        job.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelPrintJobAsync(int id)
    {
        var job = await _context.PrintJobs.FindAsync(id);
        if (job == null || job.Status != "Pending") return false;

        job.Status = "Cancelled";
        job.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Preview & Batch

    public async Task<LabelPreviewDto?> GetLabelPreviewAsync(int productId)
    {
        return await _context.Products
            .Where(p => p.Id == productId)
            .Select(p => new LabelPreviewDto
            {
                ProductId = p.Id,
                ProductSku = p.Sku,
                ProductName = p.Name,
                Price = p.SellingPrice,
                Barcode = p.Barcodes.Where(b => b.IsPrimary).Select(b => b.Barcode).FirstOrDefault(),
                CategoryName = p.Category.Name,
                UnitName = p.Unit.Name,
                BarcodeImageBase64 = "" // Would need barcode generation library
            })
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<LabelPreviewDto>> GetBatchPreviewAsync(IEnumerable<int> productIds)
    {
        return await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new LabelPreviewDto
            {
                ProductId = p.Id,
                ProductSku = p.Sku,
                ProductName = p.Name,
                Price = p.SellingPrice,
                Barcode = p.Barcodes.Where(b => b.IsPrimary).Select(b => b.Barcode).FirstOrDefault(),
                CategoryName = p.Category.Name,
                UnitName = p.Unit.Name,
                BarcodeImageBase64 = ""
            })
            .ToListAsync();
    }

    public async Task<int?> CreateBatchPrintJobAsync(BatchPrintRequest request, int userId)
    {
        var createRequest = new CreatePrintJobRequest
        {
            TemplateId = request.TemplateId,
            PrinterName = request.PrinterName,
            Items = request.ProductIds.Select(pid => new CreatePrintJobItemRequest
            {
                ProductId = pid,
                Quantity = request.QuantityPerProduct
            })
        };

        return await CreatePrintJobAsync(createRequest, userId);
    }

    #endregion

    private async Task<string> GenerateJobNoAsync()
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"PJ{today}";

        var lastJob = await _context.PrintJobs
            .Where(j => j.JobNo.StartsWith(prefix))
            .OrderByDescending(j => j.JobNo)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastJob != null && lastJob.JobNo.Length > prefix.Length)
        {
            if (int.TryParse(lastJob.JobNo.Substring(prefix.Length), out int lastSequence))
            {
                sequence = lastSequence + 1;
            }
        }

        return $"{prefix}{sequence:D4}";
    }
}
