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
using AssetManagement.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using AssetManagement.Application.Services.Implementations;

namespace AssetManagement.WebAPI.Controllers
{
    [Route("api/assets")]
    [ApiController]
    public class AssetController : BaseApiController
    {
        private readonly IAssetService _assetService;

        public AssetController(IAssetService assetService)
        {
            _assetService = assetService;
        }

        [HttpPost]
        [Authorize(Roles = RoleConstant.ADMIN)]
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
        [Authorize(Roles = RoleConstant.ADMIN)]
        public async Task<IActionResult> GetAllAssetAsync(int pageNumber, string? state, Guid? category, string? search, string? sortOrder, string? sortBy = "assetCode", string? newAssetCode = "")
        {
            try
            {
                Guid adminId = UserID;

                var assets = await _assetService.GetAllAssetAsync(adminId, pageNumber == 0 ? 1 : pageNumber, state: state, category, search, sortOrder, sortBy, "Category", newAssetCode);
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
        [Authorize]
        public async Task<IActionResult> GetAssetId(Guid id)
        {
            try
            {
                var asset = await _assetService.GetAssetByIdAsync(id);
                return Ok(new GeneralGetResponse
                {
                    Success = true,
                    Message = "Asset retrived successfully.",
                    Data = asset
                });
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

        [HttpPut("{id}")]
        [Authorize(Roles = RoleConstant.ADMIN)]
        public async Task<IActionResult> UpdateAsset(Guid id, AssetUpdateRequest assetRequest)
        {
            var response = new GeneralGetResponse();
            try
            {
                var result = await _assetService.UpdateAsset(id, assetRequest);
                response.Success = true;
                response.Message = "Update successfully";
                response.Data = result;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                return Conflict(response);
            }
        }

        [HttpPut("response/{id}")]
        [Authorize]
        public async Task<IActionResult> ResponseAsset(Guid id)
        {
            var response = new GeneralGetResponse();
            var assetRequest = new AssetUpdateRequest
            {
                Status = EnumAssetStatus.Available
            };
            try
            {
                var result = await _assetService.UpdateAsset(id, assetRequest);
                response.Success = true;
                response.Message = "Update successfully";
                response.Data = result;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                return Conflict(response);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = RoleConstant.ADMIN)]
        public async Task<IActionResult> DeleteAsset(Guid id)
        {
            try
            {
                var result = await _assetService.DeleteAssetAsync(id);
                if (result == null)
                {
                    return Conflict(new GeneralBoolResponse
                    {
                        Success = false,
                        Message = "Asset delete failed."
                    });
                }

                return Ok(new GeneralGetResponse
                {
                    Success = true,
                    Message = "Asset delete successfully.",
                    Data = result
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

        [HttpGet("reports")]
        [Authorize(Roles = RoleConstant.ADMIN)]
        public async Task<IActionResult> GetReports(int pageNumber, string? sortOrder, string? sortBy)
        {
            try
            {
                var (reports, count) = await _assetService.GetReports(sortOrder, sortBy, LocationID, pageNumber);
                if (reports.Any())
                {
                    return Ok(new GeneralGetsResponse
                    {
                        Success = true,
                        Message = "Reports retrieved successfully.",
                        Data = reports,
                        TotalCount = count
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

        [HttpGet("export-to-excel")]
        [Authorize(Roles = RoleConstant.ADMIN)]
        public async Task<IActionResult> ExportToExcel()
        {
            try
            {
                var file = await _assetService.ExportToExcelAsync(LocationID);
                return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AssetReport.xlsx");
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
    }
}
