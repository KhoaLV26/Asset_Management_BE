using AssetManagement.Application.Models.Responses;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AssetManagement.Application.Services.Implementations
{
    public class AssetService : IAssetService
    {
        private readonly IUnitOfWork _unitOfWork;
        public AssetService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public Task<AssetResponse> CreateAssetAsync(AssetRequest assetRequest)
        {
            throw new NotImplementedException();
        }

        public async void DeleteAssetByIdAsync(Guid id)
        {
            var asset = await _unitOfWork.AssetRepository.GetAsync(a => a.Id == id);
            if (asset != null)
            {
                throw new KeyNotFoundException();
            }
            _unitOfWork.AssetRepository.Delete(asset);
        }

        public async Task<(IEnumerable<AssetResponse> data, int totalCount)> GetAllAssetAsync(int page = 1, Expression<Func<Asset, bool>>? filter = null, Func<IQueryable<Asset>, IOrderedQueryable<Asset>>? orderBy = null, string includeProperties = "")
        {
            var assets = await _unitOfWork.AssetRepository.GetAllAsync(page, filter, orderBy, includeProperties);
            return (assets.items.Select(a => new AssetResponse
            {
                Id = a.Id,
                AssetCode = a.AssetCode,
                AssetName = a.AssetName,
                CategoryName = a.Category.Name,
                Status = a.Status
            }),assets.totalCount);
        }

        public async Task<AssetResponse> GetAssetByIdAsync(Guid id)
        {
            var asset = await _unitOfWork.AssetRepository.GetAsync(a => a.Id == id);
            if (asset == null)
            {
                return null;
            }
            return new AssetResponse
            {
                AssetName = asset.AssetName,
                AssetCode = asset.AssetCode,
                CategoryName = asset.Category.Name,
                Status = asset.Status
            };
    }

        public Task<AssetResponse> UpdateAssetByIdAsync(Guid id, AssetRequest assetRequest)
        {
            throw new NotImplementedException();
        }
    }
}
