using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
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
                Id = asset.Id,
                AssetName = asset.AssetName,
                AssetCode = asset.AssetCode,
                CategoryId = asset.CategoryId,
                CategoryName = asset.Category.Name,
                Specification = asset.Specification,
                InstallDate = asset.InstallDate,
                Status = asset.Status,
                LocationId = asset.LocationId.HasValue ? asset.LocationId.Value : Guid.Empty,
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

        public async Task<(IEnumerable<AssetResponse> data, int totalCount)> GetAllAssetAsync(int page = 1, Expression<Func<Asset, bool>>? filter = null, Func<IQueryable<Asset>, IOrderedQueryable<Asset>>? orderBy = null, string includeProperties = "", string? newAssetCode = "")
        {
            var assets = await _unitOfWork.AssetRepository.GetAllAsync(page, filter, orderBy, includeProperties);

            return (assets.items.Select(a => new AssetResponse
            {
                Id = a.Id,
                AssetCode = a.AssetCode,
                AssetName = a.AssetName,
                CategoryId = a.CategoryId,
                CategoryName = a.Category.Name,
                Status = a.Status,
                LocationId = a.LocationId.HasValue ? a.LocationId.Value : Guid.Empty,
            }), assets.totalCount);
        }

        public async Task<(IEnumerable<AssetResponse> data, int totalCount)> GetAllAssetAsync(Guid adminId, int pageNumber, string? state, Guid? category, string? search, string? sortOrder,
            string? sortBy = "assetCode", string includeProperties = "", string? newAssetCode = "")
        {
            Func<IQueryable<Asset>, IOrderedQueryable<Asset>>? orderBy = GetOrderQuery(sortOrder, sortBy);
            Expression<Func<Asset, bool>> filter = await GetFilterQuery(adminId, category, state, search);
            Expression<Func<Asset, bool>> prioritizeCondition = null;

            if (!string.IsNullOrEmpty(newAssetCode))
            {
                prioritizeCondition = u => u.AssetCode == newAssetCode;
            }

            var assets = await _unitOfWork.AssetRepository.GetAllAsync(pageNumber, filter, orderBy, includeProperties,
                prioritizeCondition);

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

        private async Task<Expression<Func<Asset, bool>>>? GetFilterQuery(Guid adminId, Guid? category, string? state, string? search)
        {
            var user = await _unitOfWork.UserRepository.GetAsync(x => x.Id == adminId);
            var locationId = user.LocationId;
            var nullableLocationId = (Guid?)locationId;
            // Determine the filtering criteria
            Expression<Func<Asset, bool>>? filter = null;
            var parameter = Expression.Parameter(typeof(Asset), "x");
            var conditions = new List<Expression>();
            var locationCondition = Expression.Equal(Expression.Property(parameter, nameof(Asset.LocationId)),
                Expression.Constant(nullableLocationId, typeof(Guid?)));
            conditions.Add(locationCondition);
            // Parse state parameter to enum
            if (!string.IsNullOrEmpty(state))
            {
                if (state.ToLower() != "all")
                {
                    if (Enum.TryParse<EnumAssetStatus>(state, true, out var parsedStatus))
                    {
                        var stateCondition = Expression.Equal(
                            Expression.Property(parameter, nameof(Asset.Status)),
                            Expression.Constant(parsedStatus)
                        );
                        conditions.Add(stateCondition);
                    }
                    else
                    {
                        throw new InvalidCastException("Invalid status value");
                    }
                }
            }
            else
            {
                // Default states: Available, NotAvailable, Assigned
                var availableCondition = Expression.Equal(
                    Expression.Property(parameter, nameof(Asset.Status)),
                    Expression.Constant(EnumAssetStatus.Available)
                );

                var notAvailableCondition = Expression.Equal(
                    Expression.Property(parameter, nameof(Asset.Status)),
                    Expression.Constant(EnumAssetStatus.NotAvailable)
                );

                var assignedCondition = Expression.Equal(
                    Expression.Property(parameter, nameof(Asset.Status)),
                    Expression.Constant(EnumAssetStatus.Assigned)
                );

                var defaultStateCondition = Expression.OrElse(
                    Expression.OrElse(availableCondition, notAvailableCondition),
                    assignedCondition
                );

                conditions.Add(defaultStateCondition);
            }
            // Add search conditions
            if (!string.IsNullOrEmpty(search))
            {
                var searchCondition = Expression.OrElse(
                    Expression.Call(
                        Expression.Property(parameter, nameof(Asset.AssetCode)),
                        nameof(string.Contains),
                        Type.EmptyTypes,
                        Expression.Constant(search)
                    ),
                    Expression.Call(
                        Expression.Property(parameter, nameof(Asset.AssetName)),
                        nameof(string.Contains),
                        Type.EmptyTypes,
                        Expression.Constant(search)
                    )
                );
                conditions.Add(searchCondition);
            }

            // Add category condition if necessary
            if (category.HasValue)
            {
                var categoryCondition = Expression.Equal(
                    Expression.Property(parameter, nameof(Asset.CategoryId)),
                    Expression.Constant(category)
                );
                conditions.Add(categoryCondition);
            }

            // Combine all conditions with AndAlso
            if (conditions.Any())
            {
                var combinedCondition = conditions.Aggregate((left, right) => Expression.AndAlso(left, right));
                filter = Expression.Lambda<Func<Asset, bool>>(combinedCondition, parameter);
            }

            return filter;
        }

        private Func<IQueryable<Asset>, IOrderedQueryable<Asset>>? GetOrderQuery(string? sortOrder, string? sortBy)
        {
            Func<IQueryable<Asset>, IOrderedQueryable<Asset>>? orderBy;
            switch (sortBy?.ToLower())
            {
                case "assetcode":
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.AssetCode) : x.OrderByDescending(a => a.AssetCode);
                    break;

                case "assetname":
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.AssetName) : x.OrderByDescending(a => a.AssetName);
                    break;

                case "category":
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.Category.Name) : x.OrderByDescending(a => a.Category.Name);
                    break;

                case "state":
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.Status) : x.OrderByDescending(a => a.Status);
                    break;

                default:
                    orderBy = null;
                    break;
            }
            return orderBy;
        }
    }
}