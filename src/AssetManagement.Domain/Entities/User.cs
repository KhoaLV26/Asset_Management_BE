using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Domain.Entities
{
    public class User : BaseEntity
    {
        [MaxLength(10)]
        public string StaffCode { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        public EnumGender Gender { get; set; }

        [MaxLength(100)]
        public string HashPassword { get; set; } = string.Empty;

        [MaxLength(100)]
        public string SaltPassword { get; set; } = string.Empty;

        public DateOnly DateOfBirth { get; set; }

        public DateOnly DateJoined { get; set; }
        public EnumUserStatus Status { get; set; } = EnumUserStatus.Active;
        public Guid LocationId { get; set; }
        public Location Location { get; set; }
        public Guid RoleId { get; set; }
        public Role Role { get; set; }
        public bool IsFirstLogin { get; set; } = true;
        public ICollection<Assignment> AssignmentsTo { get; set; } = new List<Assignment>();
        public ICollection<Assignment> AssignmentsBy { get; set; } = new List<Assignment>();
        public ICollection<ReturnRequest> ReturnRequestsAccepted { get; set; } = new List<ReturnRequest>();
    }
}