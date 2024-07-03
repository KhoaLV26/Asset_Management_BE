using AssetManagement.Domain.Enums;
using System;

namespace AssetManagement.Application.Models.Requests
{
    public class EditUserRequest
    {
        public DateOnly DateOfBirth { get; set; }
        public EnumGender Gender { get; set; }
        public DateOnly DateJoined { get; set; }
        public Guid RoleId { get; set; }
    }
}
