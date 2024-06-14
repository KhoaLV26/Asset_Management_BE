using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Services;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AssetManagement.WebAPI.Controllers
{
    [Route("api/assets")]
    [ApiController]
    public class AssetController : ControllerBase
    {
        private readonly IAssetService _assetService;
        private readonly IUserService _userService;

        public AssetController(IAssetService assetService, IUserService userService)
        {
            _assetService = assetService;
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAssetAsync([FromBody] AssetRequest assetRequest)
        {
            try
            {
                var asset = await _assetService.CreateAssetAsync(assetRequest);
                if (asset == null)
                {
                    return Conflict(new GeneralBoolResponse
                    {
                        Success = false,
                        Message = "Asset creation failed."
                    });
                }
                return Ok(new GeneralCreateResponse
                {
                    Success = true,
                    Message = "Asset created successfully.",
                    Data = asset
                });
            }
            catch (Exception ex)
            {
                return Conflict(new GeneralBoolResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAssetAsync(int currentPage, string? state, Guid? category, string? search, string? sortOrder, string? sortBy = "assetCode")
        {
            try
            {
                Guid adminId = Guid.Parse("CFF14216-AC4D-4D5D-9222-C951287E51C6");
                Func<IQueryable<Asset>, IOrderedQueryable<Asset>>? orderBy = GetOrderQuery(sortOrder, sortBy);
                Expression<Func<Asset, bool>>? filter = await GetFilterQuery(adminId, category, state, search);
                var assets = await _assetService.GetAllAssetAsync(page: currentPage == 0 ? 1 : currentPage,
                    filter: filter,
                    orderBy: String.IsNullOrEmpty(sortBy) ? null : orderBy,
                    includeProperties: "Category");
                if (assets.data.Any())
                {
                    return Ok(new GeneralGetsResponse
                    {
                        Success = true,
                        Message = "Assets retrieved successfully.",
                        Data = assets.data,
                        TotalCount = assets.totalCount
                    });
                }
                return Conflict(new GeneralGetsResponse
                {
                    Success = false,
                    Message = "No data.",
                });
            }
            catch (Exception ex)
            {
                return Conflict(new GeneralGetsResponse
                {
                    Success = false,
                    Message = ex.Message,
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAssetId(Guid id)
        {
            try
            {
                var asset = await _assetService.GetAssetByIdAsync(id);
                if (asset != null)
                {
                    return Ok(new GeneralGetResponse
                    {
                        Success = true,
                        Message = "Asset retrived successfully.",
                        Data = asset
                    });
                }
                else
                {
                    return Conflict(new GeneralBoolResponse
                    {
                        Success = false,
                        Message = "Asset not found."
                    });
                }
            }
            catch (Exception ex)
            {
                return Conflict(new GeneralGetResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
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

        private async Task<Expression<Func<Asset, bool>>>? GetFilterQuery(Guid adminId, Guid? category, string? state, string? search)
        {
            var locationId = await _userService.GetLocation(adminId);
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
    }
}