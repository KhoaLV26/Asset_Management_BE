using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Services;
using AssetManagement.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

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
    }
}