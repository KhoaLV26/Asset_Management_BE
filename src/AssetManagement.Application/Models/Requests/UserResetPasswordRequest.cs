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
    }
}