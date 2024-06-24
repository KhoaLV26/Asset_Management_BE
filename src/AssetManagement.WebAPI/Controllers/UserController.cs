using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Services;
using AssetManagement.Domain.Constants;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Models;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AssetManagement.WebAPI.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : BaseApiController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [Authorize(Roles = RoleConstant.ADMIN)]
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

        [HttpGet]
        [Authorize(Roles = RoleConstant.ADMIN)]
        public async Task<IActionResult> GetFilteredUsers(
            [FromQuery] string? search = "",
            [FromQuery] string? role = "",
            [FromQuery] string sortBy = "StaffCode",
            [FromQuery] string sortOrder = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] string? newStaffCode = "")
        {
            try
            {
                var adminId = UserID.ToString();
                var users = await _userService.GetFilteredUsersAsync(adminId, search, role, sortBy, sortOrder, pageNumber, newStaffCode);
                return Ok(new GeneralGetsResponse
                {
                    Success = true,
                    Message = "Successfully.",
                    Data = users.Items,
                    TotalCount = users.TotalCount
                });
            }
            catch (ArgumentException ex)
            {
                return Conflict(new GeneralGetsResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new GeneralGetsResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new GeneralGetsResponse
                {
                    Success = false,
                    Message = ex.Message,
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var user = await _userService.GetUserDetailAsync(id);
                return Ok(new GeneralGetResponse
                {
                    Data = user,
                    Message = "User retrieve successfully.",
                    Success = true
                });
            }
            catch (Exception e)
            {
                return Conflict(new GeneralGetResponse
                {
                    Success = false,
                    Message = e.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, EditUserRequest request)
        {
            try
            {
                var staffCode = await _userService.UpdateUserAsync(id, request);
                return Ok(new GeneralGetResponse
                {
                    Data = staffCode,
                    Message = "Update successfully",
                    Success = true
                });
            }
            catch (Exception e)
            {
                return Conflict(new GeneralGetResponse
                {
                    Success = false,
                    Message = e.Message
                });
            }
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = RoleConstant.ADMIN)]
        public async Task<IActionResult> DisableUser(Guid id)
        {
            try
            {
                var result = await _userService.DisableUser(id);
                if (result)
                {
                    return Ok(new GeneralBoolResponse
                    {
                        Success = true,
                        Message = "User disabled successfully."
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