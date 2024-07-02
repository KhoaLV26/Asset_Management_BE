using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Services;
using AssetManagement.Application.Services.Implementations;
using AssetManagement.Domain.Constants;
using AssetManagement.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using AssetManagement.Domain.Constants;
using AssetManagement.Domain.Entities;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using AssetManagement.Application.Models.Responses;

namespace AssetManagement.WebAPI.Controllers
{
    [Route("api/request-for-returning")]
    [ApiController]
    public class RequestReturningController : BaseApiController
    {
        private readonly IRequestReturnService _requestReturnService;
        private readonly IAssignmentService _assignmentService;

        public RequestReturningController(IRequestReturnService requestReturnService, IAssignmentService assignmentService)
        {
            _requestReturnService = requestReturnService;
            _assignmentService = assignmentService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UserCreateReturnRequest(Guid assignmentId)
        {
            Guid userId = UserID;
            var assignment = await _assignmentService.GetAssignmentDetailAsync(assignmentId);
            if (assignment == null)
            {
                return Conflict(new GeneralBoolResponse
                {
                    Success = false,
                    Message = "Assignment not found!"
                });
            }
            try
            {
                var returnResponse = await _requestReturnService.UserCreateReturnRequestAsync(assignmentId, userId);
                _requestReturnService.UserCreateReturnRequestAsync(assignmentId, userId);
                return Ok(new GeneralCreateResponse
                {
                    Success = true,
                    Message = "Create return request successfully.",
                    Data = returnResponse
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
        [Authorize]
        public async Task<IActionResult> GetReturnRequests([FromQuery] ReturnFilterRequest requestFilter)
        {
            try
            {
                var (returnRequests, totalCount) = await _requestReturnService.GetReturnRequestResponses(LocationID, requestFilter);
                return Ok(new GeneralGetsResponse
                {
                    Success = true,
                    Message = "Get return requests successfully",
                    Data = returnRequests,
                    TotalCount = totalCount
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

        [HttpPut("CompleteRequest/{id}")]
        [Authorize(Roles = RoleConstant.ADMIN)]
        public async Task<IActionResult> CompleteRequest(Guid id)
        {
            try
            {
                await _requestReturnService.CompleteReturnRequest(id);
                return Ok(new GeneralBoolResponse
                {
                    Success = true,
                    Message = "Complete return requests successfully",
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
                
        [HttpDelete("CancelRequest/{id}")]
        [Authorize(Roles = RoleConstant.ADMIN)]
        public async Task<IActionResult> CancelRequest(Guid id)
        {
            try
            {
                var result = await _requestReturnService.CancelRequest(id);
                if (result)
                {
                    return Ok(new GeneralBoolResponse
                    {
                        Success = true,
                        Message = "Request cancel successfully."
                    });
                }
                else
                {
                    return Conflict(new GeneralBoolResponse
                    {
                        Success = false,
                        Message = "User have valid assignment"
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
    }
}