using AssetManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.Models.Responses
{
    public class UserDetailResponse
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public EnumGender Gender { get; set; }
        public DateOnly DateJoined { get; set; }
        public Guid RoleId { get; set; }
        public string Username { get; set; }
        public string StaffCode { get; set; }
        public string RoleName { get; set; }
        public string LocationName { get; set; }
    }
}
