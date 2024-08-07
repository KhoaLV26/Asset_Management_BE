using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AssetManagement.Application.Services
{
    public interface IAssetService
    {
        Task<(IEnumerable<AssetResponse> data, int totalCount)> GetAllAssetAsync(Guid adminId, int pageNumber, string? state, Guid? category, string? search, string? sortOrder, string? sortBy = "assetCode", string includeProperties = "", string? newAssetCode = "", int pageSize = 10);

        Task<AssetDetailResponse> GetAssetByIdAsync(Guid id);

        Task<AssetResponse> CreateAssetAsync(AssetRequest assetRequest);

        Task<AssetResponse> DeleteAssetAsync(Guid id);

        Task<AssetResponse> UpdateAsset(Guid id, AssetUpdateRequest assetRequest);

        Task<(IEnumerable<ReportResponse>, int count)> GetReports(string? sortOrder, string? sortBy, Guid locationId, int pageNumber = 1, int pageSize = 10);

        Task<byte[]> ExportToExcelAsync(Guid locationId);
    }
}