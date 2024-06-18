using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.Models.Requests
{
    public class AssignmentRequest
    {
        public Guid Id { get; set; }
        public Guid AssignedTo { get; set; }
        public Guid AssignedBy { get; set; }
        public DateTime AssignedDate { get; set; }
        public Guid AssetId { get; set; }
        public EnumAssignmentStatus Status { get; set; } = EnumAssignmentStatus.WaitingForAcceptance;
    }
}
