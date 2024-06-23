using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.Models.Responses
{
    public class GetUserResponse
    {
        public Guid Id { get; set; }
        public string StaffCode { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public EnumGender Gender { get; set; }

        public DateOnly DateOfBirth { get; set; }

        public DateOnly DateJoined { get; set; }
        public EnumUserStatus Status { get; set; } = EnumUserStatus.Active;
        public Guid LocationId { get; set; }
        public Guid RoleId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public bool IsFirstLogin { get; set; }
    }
}