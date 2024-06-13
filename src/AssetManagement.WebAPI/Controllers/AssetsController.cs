using AssetManagement.Application.Services;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System;
using System.Linq;
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
        public async Task<IActionResult> GetAllAssetAsync(int currentPage, string? search, string? sortBy, string? sortOrder)
        {
            try
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

                var assets = await _assetService.GetAllAssetAsync(page: currentPage == 0 ? 1 : currentPage,
                    filter: String.IsNullOrEmpty(search)
                        ? null
                        : x => x.AssetCode.Contains(search) || x.AssetName.Contains(search),
                    orderBy: String.IsNullOrEmpty(sortBy) ? null : orderBy);
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAssetId(Guid id)
        {
            try
            {
                var asset = await _assetService.GetAssetByIdAsync(id);
                if (asset != null)
                {
                    return Ok(new GeneralGetResponse { 
                     Success = true,
                     Message = "Asset retrived successfully.",
                     Data = asset
                    });
                }
                else
                {
                    return NotFound(new GeneralGetResponse
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
                    Message = "Asset retrieved failed."
                });
            }
        }       
    }
}
