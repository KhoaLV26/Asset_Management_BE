using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Services;
using AssetManagement.Application.Services.Implementations;
using AssetManagement.Domain.Constants;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AssetManagement.WebAPI.Controllers
{
    [Route("api/assignments")]
    [ApiController]
    public class AssignmentController : BaseApiController
    {
        private readonly IAssignmentService _assignmentService;

        public AssignmentController(IAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }

        [HttpPost]
        [Authorize(Roles = RoleConstant.ADMIN)]
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
        [Authorize(Roles = RoleConstant.ADMIN)]
        public async Task<IActionResult> GetAllAssignmentAsync(int pageNumber, DateTime? assignedDate, string? state, string? search, string? sortOrder, string? sortBy = "assetCode", Guid? newAssignmentId = null)
        {
            try
            {
                Guid adminId = UserID;

                var assignments = await _assignmentService.GetAllAssignmentAsync(pageNumber == 0 ? 1 : pageNumber, state: state, assignedDate, search, sortOrder, sortBy, "UserTo,UserBy,Asset", newAssignmentId);
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

        [HttpGet("{id}")]
        [Authorize(Roles = RoleConstant.ADMIN)]
        public async Task<IActionResult> GetAssignmentDetail(Guid id)
        {
            try
            {
                var assignment = await _assignmentService.GetAssignmentDetailAsync(id);
                if (assignment != null)
                {
                    return Ok(new GeneralGetResponse
                    {
                        Success = true,
                        Message = "Assignment retrived successfully.",
                        Data = assignment
                    });
                }
                else
                {
                    return Conflict(new GeneralBoolResponse
                    {
                        Success = false,
                        Message = "Assignment not found."
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

        [HttpDelete("{id}")]
        [Authorize(Roles = RoleConstant.ADMIN)]
        public async Task<IActionResult> DeleteAssignment(Guid id)
        {
            try
            {
                var result = await _assignmentService.DeleteAssignment(id);
                if (result)
                {
                    return Ok(new GeneralBoolResponse
                    {
                        Success = true,
                        Message = "Assignment deleted successfully."
                    });
                }
                else
                {
                    return Conflict(new GeneralBoolResponse
                    {
                        Success = false,
                        Message = "Assignment not found."
                    });
                }
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

        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetUserAssignmentAsync(int pageNumber, string? sortOrder, string? sortBy = "assigneddate")
        {
            try
            {
                Guid userId = UserID;

                var assignments = await _assignmentService.GetUserAssignmentAsync(pageNumber == 0 ? 1 : pageNumber, userId, sortOrder, sortBy);
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


        [HttpPut("{id}")]
        [Authorize(Roles = RoleConstant.ADMIN)]
        public async Task<IActionResult> UpdateAssignment(Guid id, [FromBody] AssignmentRequest assignmentRequest)
        {
            var response = new GeneralBoolResponse();
            try
            {
                var result = await _assignmentService.UpdateAssignment(id, assignmentRequest);
                if (result == false)
                {
                    response.Success = false;
                    response.Message = "Update fail";
                    return Conflict(response);
                }
                response.Success = true;
                response.Message = "Update successfully";
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                return Conflict(response);
            }
        }

        [HttpGet("user/{userId}")]
        [Authorize(Roles = RoleConstant.ADMIN)]
        public async Task<IActionResult> AdminGetUserAssignmentAsync(int pageNumber, Guid userId)
        {
            try
            {
                var assignments = await _assignmentService.GetUserAssignmentAsync(pageNumber == 0 ? 1 : pageNumber, userId, "", "");
                if (assignments.data.Any())
                {
                    return Ok(new GeneralGetsResponse
                    {
                        Success = true,
                        Message = "Assignments of user retrieved successfully.",
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
