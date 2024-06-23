using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Interfaces;
using AutoMapper;
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
        private readonly IMapper _mapper;

        public AssetService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AssetResponse> CreateAssetAsync(AssetRequest assetRequest)
        {
            var adminCreated = await _unitOfWork.UserRepository.GetAsync(u => u.Id == assetRequest.CreatedBy, u => u.Location);
            if (adminCreated == null)
            {
                throw new KeyNotFoundException("Admin not found");
            }

            var category = await _unitOfWork.CategoryRepository.GetAsync(c => c.Id == assetRequest.CategoryId);
            if (category == null)
            {
                throw new KeyNotFoundException("Category not found");
            }

            var prefix = category.Code.ToUpper();
            var assetNumber = await GenerateAssetCodeAsync(prefix);

            var newAsset = new Asset
            {
                AssetCode = $"{prefix}{assetNumber:D6}",
                AssetName = assetRequest.AssetName,
                CategoryId = assetRequest.CategoryId,
                Category = category,
                Status = assetRequest.Status,
                CreatedBy = assetRequest.CreatedBy,
                LocationId = adminCreated.LocationId,
                Location = adminCreated.Location,
                Specification = assetRequest.Specification,
                InstallDate = assetRequest.InstallDate,
            };

            await _unitOfWork.AssetRepository.AddAsync(newAsset);
            if (await _unitOfWork.CommitAsync() > 0)
            {
                return _mapper.Map<AssetResponse>(newAsset);
            }
            else
            {
                throw new Exception("Failed to create asset");
            }
        }

        private async Task<int> GenerateAssetCodeAsync(string prefix)
        {
            var assetsWithPrefix = await _unitOfWork.AssetRepository
                    .GetAllAsync(a => a.AssetCode.Length == 8 && a.Category.Code.StartsWith(prefix), a => a.Category);
            var latestAsset = assetsWithPrefix.OrderByDescending(a => a.AssetCode).FirstOrDefault();
            var assetNumber = 1;

            if (latestAsset != null)
            {
                assetNumber = int.Parse(latestAsset.AssetCode.Substring(prefix.Length)) + 1;
            }

            return assetNumber;
        }

        public async Task<(IEnumerable<AssetResponse> data, int totalCount)> GetAllAssetAsync(int page = 1, Expression<Func<Asset, bool>>? filter = null, Func<IQueryable<Asset>, IOrderedQueryable<Asset>>? orderBy = null, string includeProperties = "")
        {
            var assets = await _unitOfWork.AssetRepository.GetAllAsync(page, filter, orderBy, includeProperties);

            return (assets.items.Select(a => new AssetResponse
            {
                Id = a.Id,
                AssetCode = a.AssetCode,
                AssetName = a.AssetName,
                CategoryId = a.CategoryId,
                CategoryName = a.Category.Name,
                Status = a.Status
            }), assets.totalCount);
        }

        public async Task<AssetDetailResponse> GetAssetByIdAsync(Guid id)
        {
            var asset = await _unitOfWork.AssetRepository.GetAssetDetail(id);
            if (asset == null)
            {
                return null;
            }
            var assignmentResponses = asset.Assignments.Select(a => new AssignmentResponse
            {
                Id = a.Id,
                AssetId = a.AssetId,
                AssignedBy = a.AssignedBy,
                AssignedTo = a.AssignedTo,
                AssignedDate = a.AssignedDate,
                Status = a.Status,
                By = a.UserBy.Username,
                To = a.UserTo.Username
            }).ToList();

            return new AssetDetailResponse
            {
                AssetName = asset.AssetName,
                AssetCode = asset.AssetCode,
                CategoryId = asset.CategoryId,
                CategoryName = asset.Category.Name,
                Specification = asset.Specification,
                InstallDate = asset.InstallDate,
                Status = asset.Status,
                AssignmentResponses = assignmentResponses.Select(a => new AssignmentResponse
                {
                    Id = a.Id,
                    AssetId = a.AssetId,
                    AssignedBy = a.AssignedBy,
                    AssignedTo = a.AssignedTo,
                    AssignedDate = a.AssignedDate,
                    Status = a.Status,
                    By = a.By,
                    To = a.To
                }).ToList()
            };
        }
    }
}