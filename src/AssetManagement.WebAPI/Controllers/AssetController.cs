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
        public async Task<IActionResult> GetAllAssetAsync(int pageNumber, string? state, Guid? category, string? search, string? sortOrder, string? sortBy = "assetCode", string? newAssetCode = "")
        {
            try
            {
                Guid adminId = Guid.Parse("CFF14216-AC4D-4D5D-9222-C951287E51C6");
                
                var assets = await _assetService.GetAllAssetAsync(adminId,pageNumber == 0 ? 1 : pageNumber, state:state,category,search,sortOrder,sortBy,"Category",newAssetCode);
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

    }
}