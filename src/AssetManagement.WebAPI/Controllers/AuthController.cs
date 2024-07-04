using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Services;
using AssetManagement.Domain.Models;
using AssetManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AssetManagement.WebAPI.Controllers
{
    [ApiController]
    [Route("api/auths")]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;

        public AuthController(IAuthService authService, IEmailService emailService)
        {
            _authService = authService;
            _emailService = emailService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] UserLoginRequest request)
        {
            try
            {
                var (token, refreshToken, user) = await _authService.LoginAsync(request.Username, request.Password);
                var response = new GeneralGetResponse
                {
                    Message = "User logged in successfully",
                    Data = new { token, refreshToken, user }
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new GeneralBoolResponse
                {
                    Success = false,
                    Message = ex.Message
                };
                return Conflict(response);
            }
        }

        [HttpPost("reset-password")]
        [Authorize]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] UserResetPasswordRequest request)
        {
            try
            {
                await _authService.ResetPasswordAsync(request.Username, request.Password, request.RefreshToken);
                var response = new GeneralBoolResponse
                {
                    Message = "Password reset successfully",
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new GeneralBoolResponse
                {
                    Success = false,
                    Message = ex.Message
                };
                return Conflict(response);
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordRequest request)
        {
            try
            {
                await _authService.ChangePasswordAsync(request.Username, request.OldPassword, request.NewPassword, request.RefreshToken, CurrentToken);
                var response = new GeneralBoolResponse
                {
                    Message = "Password changed successfully",
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new GeneralBoolResponse
                {
                    Success = false,
                    Message = ex.Message
                };
                return Conflict(response);
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest refreshTokenRequest)
        {
            try
            {
                var (token, refreshToken, user) = await _authService.RefreshTokenAsync(refreshTokenRequest.RefreshToken);
                var response = new GeneralGetResponse
                {
                    Message = "Token refreshed successfully",
                    Data = new { token, refreshToken, user }
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new GeneralBoolResponse
                {
                    Success = false,
                    Message = ex.Message
                };
                return Conflict(response);
            }
        }
    }
}