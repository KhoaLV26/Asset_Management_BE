using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Services;
using AssetManagement.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System;
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
    }
}
