﻿using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Services;
using AssetManagement.Domain.Constants;
using AssetManagement.Domain.Entities;
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

        //[HttpPost("register")]
        //public async Task<IActionResult> RegisterAsync([FromBody] UserRegisterRequest request)
        //{
        //    try
        //    {
        //        var result = await _authService.RegisterAsync(request.Email, request.Password, request.RoleId);
        //        var response = new GeneralGetResponse
        //        {
        //            Message = "User registered successfully",
        //            Data = result
        //        };
        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Conflict(ex.Message);
        //    }
        //}

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

        [HttpGet("logout")]
        [Authorize]
        public async Task<IActionResult> LogoutAsync()
        {
            try
            {
                if (await _authService.LogoutAsync(UserID) == 0)
                {
                    throw new InvalidOperationException("User not found");
                }
                var response = new GeneralBoolResponse
                {
                    Message = "User logged out successfully"
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        //[HttpGet("request-reset-password/{email}")]
        //public async Task<IActionResult> RequestResetPasswordAsync(string email)
        //{
        //    try
        //    {
        //        var user = await _userService.GetUserByEmailAsync(email);
        //        if (user == null)
        //        {
        //            throw new KeyNotFoundException("User not found");
        //        }
        //        var isSuccess = await _emailService.SendEmailAsync(user.Email, EmailConstants.SUBJECT_RESET_PASSWORD, EmailConstants.BodyResetPasswordEmail(email));
        //        if (!isSuccess)
        //        {
        //            throw new InvalidOperationException("Failed to send email");
        //        }
        //        var response = new GeneralBoolResponse
        //        {
        //            Message = "Reset password email sent successfully"
        //        };
        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Conflict(ex.Message);
        //    }
        //}

        [HttpPost("reset-password")]
        [Authorize]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] UserResetPasswordRequest request)
        {
            try
            {
                await _authService.ResetPasswordAsync(request.Username, request.Password);
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
                await _authService.ChangePasswordAsync(request.Username, request.OldPassword, request.NewPassword);
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