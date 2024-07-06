using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Services;
using AssetManagement.Domain.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AssetManagement.WebAPI.Controllers
{
    [Route("api/locations")]
    [ApiController]
    public class LocationController : BaseApiController
    {
        private readonly ILocationService _locationService;

        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllLocationAsync([FromQuery] int pageNumber, [FromQuery] string? search, [FromQuery] string? sortOrder, [FromQuery] string? sortBy = "Name", [FromQuery] string? newLocationCode = "", [FromQuery] int pageSize = 10)
        {
            try
            {
                var (data, totalCount) = await _locationService.GetAllLocationAsync(pageNumber, search, sortOrder, sortBy, newLocationCode, pageSize);
                return Ok(new GeneralGetsResponse
                {
                    Success = true,
                    Message = "Successfully.",
                    Data = data,
                    TotalCount = totalCount
                });
            }
            catch (Exception ex)
            {
                return Conflict(new GeneralGetsResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateLocationAsync([FromBody] LocationCreateRequest request)
        {
            try
            {
                var locationResponse = await _locationService.CreateLocationAsync(request);

                return Ok(new GeneralCreateResponse
                {
                    Success = true,
                    Message = "Location created successfully.",
                    Data = locationResponse
                });
            }
            catch (Exception ex)
            {
                return Conflict(new GeneralCreateResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPut("{locationId}")]
        [Authorize]
        public async Task<IActionResult> UpdateLocationAsync(Guid locationId, [FromBody] LocationUpdateRequest request)
        {
            try
            {
                var locationResponse = await _locationService.UpdateLocationAsync(locationId, request);

                return Ok(new GeneralCreateResponse
                {
                    Success = true,
                    Message = "Location updated successfully.",
                    Data = locationResponse
                });
            }
            catch (Exception ex)
            {
                return Conflict(new GeneralCreateResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("{locationId}")]
        [Authorize]
        public async Task<IActionResult> GetLocationByIdAsync(Guid locationId)
        {
            try
            {
                var locationResponse = await _locationService.GetLocationByIdAsync(locationId);

                return Ok(new GeneralGetsResponse
                {
                    Success = true,
                    Message = "Successfully.",
                    Data = locationResponse
                });
            }
            catch (Exception ex)
            {
                return Conflict(new GeneralGetsResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}