using AssetManagement.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Application.Models.Requests
{
    public class EditUserRequest
    {
        [MaxLength(50)]
        public string FirstName { get; set; }
        [MaxLength(50)]
        public string LastName { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public EnumGender Gender { get; set; }
        public DateOnly DateJoined { get; set; }
        public Guid RoleId { get; set; }
    }
}
