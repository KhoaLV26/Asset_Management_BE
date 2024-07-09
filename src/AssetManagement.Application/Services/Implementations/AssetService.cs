using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Domain.Constants;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Interfaces;
using AutoMapper;
using ClosedXML.Excel;
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
                LocationId = asset.LocationId.Value,
                AssignmentResponses = asset.Assignments.Select(a => new AssignmentResponse
                {
                    Id = a.Id,
                    AssetId = a.AssetId,
                    AssignedBy = a.AssignedBy,
                    AssignedTo = a.AssignedTo,
                    AssignedDate = a.AssignedDate,
                    Status = a.Status,
                    AssignedByName = a.UserBy.Username,
                    AssignedToName = a.UserTo.Username
                }).Where(a => a.Status == EnumAssignmentStatus.Accepted || a.Status == EnumAssignmentStatus.Returned).ToList()
            };
        }

        public async Task<(IEnumerable<AssetResponse> data, int totalCount)> GetAllAssetAsync(int page = 1, Expression<Func<Asset, bool>>? filter = null, Func<IQueryable<Asset>, IOrderedQueryable<Asset>>? orderBy = null, string includeProperties = "", string? newAssetCode = "", int pageSize = 10)
        {
            var assets = await _unitOfWork.AssetRepository.GetAllAsync(page, filter, orderBy, includeProperties);

            return (assets.items.Select(a => new AssetResponse
            {
                Id = a.Id,
                AssetCode = a.AssetCode,
                AssetName = a.AssetName,
                CategoryId = a.CategoryId,
                Specification = a.Specification,
                CategoryName = a.Category.Name,
                InstallDate = a.InstallDate,
                Status = a.Status,
                LocationId = a.LocationId.HasValue ? a.LocationId.Value : Guid.Empty,
            }), assets.totalCount);
        }

        public async Task<(IEnumerable<AssetResponse> data, int totalCount)> GetAllAssetAsync(Guid adminId, int pageNumber, string? state, Guid? category, string? search, string? sortOrder,
            string? sortBy = "assetCode", string includeProperties = "", string? newAssetCode = "", int pageSize = 10)
        {
            Func<IQueryable<Asset>, IOrderedQueryable<Asset>>? orderBy = GetOrderQuery(sortOrder, sortBy);
            Expression<Func<Asset, bool>> filter = await GetFilterQuery(adminId, category, state, search);
            Expression<Func<Asset, bool>> prioritizeCondition = null;

            if (!string.IsNullOrEmpty(newAssetCode))
            {
                prioritizeCondition = u => u.AssetCode == newAssetCode;
            }

            var assets = await _unitOfWork.AssetRepository.GetAllAsync(pageNumber, filter, orderBy, includeProperties,
                prioritizeCondition, pageSize);

            return (assets.items.Select(a => new AssetResponse
            {
                Id = a.Id,
                AssetCode = a.AssetCode,
                AssetName = a.AssetName,
                CategoryId = a.CategoryId,
                CategoryName = a.Category.Name,
                Specification = a.Specification,
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

            // Add IsDelete
            var isDeletedCondition = Expression.Equal(Expression.Property(parameter, nameof(Asset.IsDeleted)),
                Expression.Constant(false));
            conditions.Add(isDeletedCondition);

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

        public async Task<AssetResponse> DeleteAssetAsync(Guid id)
        {
            var asset = await _unitOfWork.AssetRepository.GetAsync(x => x.Id == id);
            if (asset == null)
            {
                throw new Exception("Asset not found");
            }

            var detail = await GetAssetByIdAsync(id);
            if (detail.AssignmentResponses != null && detail.AssignmentResponses.Any())
            {
                throw new Exception("This asset have historical assignment");
            }
            else
            {
                _unitOfWork.AssetRepository.SoftDelete(asset);
                if (await _unitOfWork.CommitAsync() > 0)
                {
                    return _mapper.Map<AssetResponse>(asset);
                }
                else
                {
                    throw new Exception("Failed to delete asset");
                }
            }
        }

        private Func<IQueryable<Asset>, IOrderedQueryable<Asset>>? GetOrderQuery(string? sortOrder, string? sortBy)
        {
            Func<IQueryable<Asset>, IOrderedQueryable<Asset>>? orderBy;
            switch (sortBy?.ToLower())
            {
                case SortConstants.Asset.SORT_BY_ASSET_CODE:
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.AssetCode) : x.OrderByDescending(a => a.AssetCode);
                    break;

                case SortConstants.Asset.SORT_BY_ASSET_NAME:
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.AssetName) : x.OrderByDescending(a => a.AssetName);
                    break;

                case SortConstants.Asset.SORT_BY_CATEGORY:
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.Category.Name) : x.OrderByDescending(a => a.Category.Name);
                    break;

                case SortConstants.Asset.SORT_BY_STATE:
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.Status) : x.OrderByDescending(a => a.Status);
                    break;

                default:
                    orderBy = null;
                    break;
            }
            return orderBy;
        }

        public async Task<AssetResponse> UpdateAsset(Guid id, AssetUpdateRequest assetRequest)
        {
            var currentAsset = await _unitOfWork.AssetRepository.GetAsync(x => x.Id == id && !x.IsDeleted);
            if (currentAsset == null)
            {
                throw new ArgumentException("Asset not exist");
            }

            currentAsset.AssetName = assetRequest.AssetName == string.Empty ? currentAsset.AssetName : assetRequest.AssetName;
            currentAsset.Specification = assetRequest.Specification == string.Empty ? currentAsset.Specification : assetRequest.Specification;
            currentAsset.InstallDate = assetRequest.InstallDate == DateOnly.MinValue ? currentAsset.InstallDate : assetRequest.InstallDate;
            currentAsset.Status = Enum.IsDefined(typeof(EnumAssetStatus), assetRequest.Status) ? assetRequest.Status : currentAsset.Status;

            await _unitOfWork.CommitAsync();
        
            return new AssetResponse
            {
                AssetCode = currentAsset.AssetCode,
            };
        }


        public async Task<(IEnumerable<ReportResponse>, int count)> GetReports(string? sortOrder, string? sortBy, Guid locationId, int pageNumber = 1, int pageSize = 10)
        {
            var categories = await _unitOfWork.CategoryRepository.GetAllAsync(c => !c.IsDeleted);
            var assets = await _unitOfWork.AssetRepository.GetAllAsync(a => a.LocationId == locationId && !a.IsDeleted);

            var reports = categories.Select(category => new ReportResponse
            {
                Category = category.Name,
                Total = assets.Count(asset => asset.CategoryId == category.Id),
                Assigned = assets.Count(asset => asset.CategoryId == category.Id && asset.Status == EnumAssetStatus.Assigned),
                Available = assets.Count(asset => asset.CategoryId == category.Id && asset.Status == EnumAssetStatus.Available),
                NotAvailable = assets.Count(asset => asset.CategoryId == category.Id && asset.Status == EnumAssetStatus.NotAvailable),
                WaitingForRecycling = assets.Count(asset => asset.CategoryId == category.Id && asset.Status == EnumAssetStatus.WaitingForRecycling),
                Recycled = assets.Count(asset => asset.CategoryId == category.Id && asset.Status == EnumAssetStatus.Recycled)
            }).AsQueryable();
            sortBy ??= "Category";
            var orderBy = GetOrderReportQuery(sortOrder, sortBy);
            if (orderBy != null)
            {
                reports = orderBy(reports);
            }
            reports = reports.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            return (reports, categories.Count());
        }

        private Func<IQueryable<ReportResponse>, IOrderedQueryable<ReportResponse>>? GetOrderReportQuery(string? sortOrder, string? sortBy)
        {
            Func<IQueryable<ReportResponse>, IOrderedQueryable<ReportResponse>>? orderBy;
            switch (sortBy?.ToLower())
            {
                case SortConstants.Report.SORT_BY_TOTAL:
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.Total) : x.OrderByDescending(a => a.Total);
                    break;

                case SortConstants.Report.SORT_BY_ASSIGNED:
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.Assigned) : x.OrderByDescending(a => a.Assigned);
                    break;

                case SortConstants.Report.SORT_BY_AVAILABLE:
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.Available) : x.OrderByDescending(a => a.Available);
                    break;

                case SortConstants.Report.SORT_BY_NOT_AVAILABLE:
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.NotAvailable) : x.OrderByDescending(a => a.NotAvailable);
                    break;

                case SortConstants.Report.SORT_BY_WAITING_FOR_RECYCLING:
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.WaitingForRecycling) : x.OrderByDescending(a => a.WaitingForRecycling);
                    break;

                case SortConstants.Report.SORT_BY_RECYCLED:
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.Recycled) : x.OrderByDescending(a => a.Recycled);
                    break;

                case SortConstants.Report.SORT_BY_CATEGORY:
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.Category) : x.OrderByDescending(a => a.Category);
                    break;

                default:
                    orderBy = null;
                    break;
            }
            return orderBy;
        }

        public async Task<byte[]> ExportToExcelAsync(Guid locationId)
        {
            var categories = await _unitOfWork.CategoryRepository.GetAllAsync(c => !c.IsDeleted);
            var assets = await _unitOfWork.AssetRepository.GetAllAsync(a => a.LocationId == locationId && !a.IsDeleted);

            var reports = categories.Select(category => new ReportResponse
            {
                Category = category.Name,
                Total = assets.Count(asset => asset.CategoryId == category.Id),
                Assigned = assets.Count(asset => asset.CategoryId == category.Id && asset.Status == EnumAssetStatus.Assigned),
                Available = assets.Count(asset => asset.CategoryId == category.Id && asset.Status == EnumAssetStatus.Available),
                NotAvailable = assets.Count(asset => asset.CategoryId == category.Id && asset.Status == EnumAssetStatus.NotAvailable),
                WaitingForRecycling = assets.Count(asset => asset.CategoryId == category.Id && asset.Status == EnumAssetStatus.WaitingForRecycling),
                Recycled = assets.Count(asset => asset.CategoryId == category.Id && asset.Status == EnumAssetStatus.Recycled)
            }).OrderBy(r => r.Category);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Reports");

                var properties = typeof(ReportResponse).GetProperties();

                // Add headers
                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = properties[i].Name;
                }

                // Add data
                int row = 2;
                foreach (var report in reports)
                {
                    worksheet.Cell(row, 1).Value = report.Category;
                    worksheet.Cell(row, 2).Value = report.Total;
                    worksheet.Cell(row, 3).Value = report.Available;
                    worksheet.Cell(row, 4).Value = report.NotAvailable;
                    worksheet.Cell(row, 5).Value = report.Assigned;
                    worksheet.Cell(row, 6).Value = report.WaitingForRecycling;
                    worksheet.Cell(row, 7).Value = report.Recycled;
                    row++;
                }

                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
}
