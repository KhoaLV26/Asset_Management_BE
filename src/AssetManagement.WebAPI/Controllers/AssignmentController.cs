using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Services;
using AssetManagement.Application.Services.Implementations;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AssetManagement.WebAPI.Controllers
{
    [Route("api/assignments")]
    [ApiController]
    public class AssignmentController : ControllerBase
    {
        private readonly IAssignmentService _assignmentService;
        public AssignmentController(IAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateAssignment([FromBody] AssignmentRequest assignmentRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new GeneralCreateResponse
                {
                    Success = false,
                    Message = "Invalid request data.",
                    Data = ModelState
                });
            }
            try
            {
                var assignmentResponse = await _assignmentService.AddAssignmentAsync(assignmentRequest);
                return Ok(new GeneralCreateResponse
                {
                    Success = true,
                    Message = "Create assignment successfully.",
                    Data = assignmentResponse
                });
            }
            catch (ArgumentException ex)
            {
                return Conflict(new GeneralCreateResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new GeneralBoolResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new GeneralBoolResponse
                {
                    Success = false,
                    Message = "An error occurred while registering the user.",
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAssignmentAsync(int pageNumber, string? state, string? search, string? sortOrder, string? sortBy = "assetCode", string? newAssetCode = "")
        {
            try
            {
                Guid adminId = Guid.Parse("CFF14216-AC4D-4D5D-9222-C951287E51C6");

                var assignments = await _assignmentService.GetAllAssignmentAsync(adminId, pageNumber == 0 ? 1 : pageNumber, state: state, search, sortOrder, sortBy, "UserTo,UserBy,Asset", newAssetCode);
                if (assignments.data.Any())
                {
                    return Ok(new GeneralGetsResponse
                    {
                        Success = true,
                        Message = "Assignments retrieved successfully.",
                        Data = assignments.data,
                        TotalCount = assignments.totalCount
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
    }
}
