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
    [Route("api/[controller]")]
    [ApiController]
    public class AssetsController : ControllerBase
    {
        private readonly IAssetService _assetService;

        public AssetsController(IAssetService assetService)
        {
            _assetService = assetService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAssetAsync(int currentPage, string? state, Guid? category, string? search, string? sortBy, string? sortOrder)
        {
            try
            {
                Func<IQueryable<Asset>, IOrderedQueryable<Asset>>? orderBy = GetOrderQuery(sortOrder, sortBy);
                Expression<Func<Asset, bool>>? filter = GetFilterQuery(category, state, search);
                var assets = await _assetService.GetAllAssetAsync(page: currentPage == 0 ? 1 : currentPage,
                    filter: filter,
                    orderBy: String.IsNullOrEmpty(sortBy) ? null : orderBy,
                    includeProperties:"Category");
                return Ok(new GeneralGetsResponse
                {
                    Success = true,
                    Message = "Assets retrieved successfully.",
                    Data = assets.data,
                    TotalCount = assets.totalCount
                });
            }
            catch (Exception ex)
            {
                return Conflict(new GeneralGetsResponse
                {
                    Success = false,
                    Message = "Assets retrieved failed.",
                });
            }
        }

        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetAssetId(Guid id)
        //{
        //    try
        //    {
        //        var asset = await _assetService.GetAssetByIdAsync(id);
        //        if (asset != null)
        //        {
        //            return Ok(new GeneralGetResponse
        //            {
        //                Success = true,
        //                Message = "Asset retrived successfully.",
        //                Data = asset
        //            });
        //        }
        //        else
        //        {
        //            return NotFound(new GeneralGetResponse
        //            {
        //                Success = false,
        //                Message = "Asset not found."
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Conflict(new GeneralGetResponse
        //        {
        //            Success = false,
        //            Message = "Asset retrieved failed."
        //        }); 
        //    }
        //}

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
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.CategoryId) : x.OrderByDescending(a => a.CategoryId);
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

        private Expression<Func<Asset, bool>>? GetFilterQuery(Guid? category, string? state, string? search)
        {
            // Determine the filtering criteria
            Expression<Func<Asset, bool>>? filter = null;
            var parameter = Expression.Parameter(typeof(Asset), "x");
            var conditions = new List<Expression>();

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
