using AssetManagement.Domain.Constants;
using AssetManagement.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Application.Models.Requests
{
    public class UserRegisterRequest
    {
        [Required]
        [RegularExpression(@"^[a-zA-Z]+([a-zA-Z ]*[a-zA-Z])?$", ErrorMessage = "First name can only contain letters and spaces, and cannot start or end with a space.")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[a-zA-Z]+([a-zA-Z ]*[a-zA-Z])?$", ErrorMessage = "First name can only contain letters and spaces, and cannot start or end with a space.")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        public DateOnly DateJoined { get; set; }

        [Required]
        public EnumGender Gender { get; set; }

        [Required]
        public Guid RoleId { get; set; }

        public Guid CreateBy { get; set; }
    }
}