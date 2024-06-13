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
        Task<(IEnumerable<AssetResponse> data, int totalCount)> GetAllAssetAsync(int page = 1, Expression<Func<Asset, bool>>? filter = null, Func<IQueryable<Asset>, IOrderedQueryable<Asset>>? orderBy = null, string includeProperties = "");
        Task<AssetDetailResponse> GetAssetByIdAsync(Guid id);
        void DeleteAssetByIdAsync(Guid id);
        Task<AssetResponse> CreateAssetAsync(AssetRequest assetRequest);
        Task<AssetResponse> UpdateAssetByIdAsync(Guid id, AssetRequest assetRequest);
    }
}
