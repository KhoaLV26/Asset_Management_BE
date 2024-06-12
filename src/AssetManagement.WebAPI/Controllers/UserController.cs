using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Services;
using AssetManagement.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AssetManagement.WebAPI.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] UserRegisterRequest request)
        {
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
                    var userResponse = await _userService.AddUserAsync(request);

                    return Ok(new GeneralCreateResponse
                    {
                        Success = true,
                        Message = "User registered successfully.",
                        Data = userResponse
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
}