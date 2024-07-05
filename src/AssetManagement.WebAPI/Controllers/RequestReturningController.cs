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

        public RequestReturningController(IRequestReturnService requestReturnService)
        {
            _requestReturnService = requestReturnService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UserCreateReturnRequest(Guid assignmentId)
        {
            Guid userId = UserID;
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

        [HttpPost("{assignmentId}")]
        [Authorize(Roles = RoleConstant.ADMIN)]
        public async Task<IActionResult> CreateReturnRequestAsync(Guid assignmentId)
        {
            try
            {
                Guid adminId = UserID;
                var returnRequest = await _requestReturnService.AddReturnRequestAsync(adminId, assignmentId);
                if (returnRequest == null)
                {
                    return Conflict(new GeneralBoolResponse
                    {
                        Success = false,
                        Message = "Return Request creation failed."
                    });
                }
                return Ok(new GeneralCreateResponse
                {
                    Success = true,
                    Message = "Return Request created successfully.",
                    Data = returnRequest
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
        [Authorize]
        public async Task<IActionResult> GetReturnRequests([FromQuery] ReturnFilterRequest requestFilter, int pageSize = 10)
        {
            try
            {
                var (returnRequests, totalCount) = await _requestReturnService.GetReturnRequestResponses(LocationID, requestFilter, pageSize);
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
                await _requestReturnService.CompleteReturnRequest(id, UserID);
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
                        Message = "Request cancel failed"
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
