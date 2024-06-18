using AssetManagement.Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Application.Models.Requests
{
    public class UserResetPasswordRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [RegularExpression(RegexConstants.PASSWORD, ErrorMessage = ErrorMessage.ERROR_PASSWORD)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password", ErrorMessage = "Confirm password must be same New Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}