using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Domain.Entities
{
    public class Assignment : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid AssignedTo { get; set; }
        public User UserTo { get; set; }
        public Guid AssignedBy { get; set; }
        public User UserBy { get; set; }
        public DateTime AssignedDate { get; set; }
        public Guid AssetId { get; set; }
        public Asset Asset { get; set; }
        public Guid? ReturnRequestId { get; set; }
        public ReturnRequest? ReturnRequest { get; set; }
        public EnumAssignmentStatus Status { get; set; } = EnumAssignmentStatus.WaitingForAcceptance;
    }
}