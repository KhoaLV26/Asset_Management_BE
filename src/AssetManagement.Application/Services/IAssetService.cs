using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Models.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using AssetManagement.Domain.Entities;

namespace AssetManagement.Application.Services
{
    public interface IAssetService
    {
        Task<(IEnumerable<AssetResponse> data,int totalCount)> GetAllAssetAsync(int page = 1, Expression<Func<Asset, bool>>? filter = null, Func<IQueryable<Asset>, IOrderedQueryable<Asset>>? orderBy = null, string includeProperties = "");
        Task<AssetResponse> GetAssetByIdAsync(Guid id);
        void DeleteAssetByIdAsync(Guid id);
        Task<AssetResponse> CreateAssetAsync(AssetRequest assetRequest);
        Task<AssetResponse> UpdateAssetByIdAsync(Guid id,  AssetRequest assetRequest);
    }
}
