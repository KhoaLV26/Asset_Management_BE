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
        [Authorize(Roles = RoleConstant.ADMIN)]
        public async Task<IActionResult> CreateReturnRequestAsync([FromBody] Guid assignmentId)
        {
            try
            {
                var returnRequest = await _requestReturnService.AddReturnRequestAsync(assignmentId);
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
                
        [HttpDelete("{id}")]
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