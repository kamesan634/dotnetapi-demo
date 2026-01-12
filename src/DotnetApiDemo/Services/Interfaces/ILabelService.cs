using DotnetApiDemo.Models.DTOs.Common;
using DotnetApiDemo.Models.DTOs.Labels;

namespace DotnetApiDemo.Services.Interfaces;

public interface ILabelService
{
    // Label Templates
    Task<PaginatedResponse<LabelTemplateListDto>> GetTemplatesAsync(PaginationRequest request, string? type = null);
    Task<LabelTemplateDetailDto?> GetTemplateByIdAsync(int id);
    Task<IEnumerable<LabelTemplateListDto>> GetActiveTemplatesAsync(string? type = null);
    Task<int?> CreateTemplateAsync(CreateLabelTemplateRequest request);
    Task<bool> UpdateTemplateAsync(int id, UpdateLabelTemplateRequest request);
    Task<bool> DeleteTemplateAsync(int id);

    // Print Jobs
    Task<PaginatedResponse<PrintJobListDto>> GetPrintJobsAsync(PaginationRequest request, string? status = null);
    Task<PrintJobDetailDto?> GetPrintJobByIdAsync(int id);
    Task<int?> CreatePrintJobAsync(CreatePrintJobRequest request, int userId);
    Task<bool> StartPrintJobAsync(int id);
    Task<bool> CompletePrintJobAsync(int id, int printedCount);
    Task<bool> FailPrintJobAsync(int id, string errorMessage);
    Task<bool> CancelPrintJobAsync(int id);

    // Preview & Batch
    Task<LabelPreviewDto?> GetLabelPreviewAsync(int productId);
    Task<IEnumerable<LabelPreviewDto>> GetBatchPreviewAsync(IEnumerable<int> productIds);
    Task<int?> CreateBatchPrintJobAsync(BatchPrintRequest request, int userId);
}
