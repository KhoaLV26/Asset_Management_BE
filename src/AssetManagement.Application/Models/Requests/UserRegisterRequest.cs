using AssetManagement.Domain.Constants;
using AssetManagement.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Application.Models.Requests
{
    public class UserRegisterRequest
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        public DateOnly DateJoined { get; set; }

        [Required]
        public EnumGender Gender { get; set; }

        [Required]
        public Guid RoleId { get; set; }
    }
}