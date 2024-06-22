using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.Models.Responses
{
    public class AssignmentResponse
    {
        public Guid Id { get; set; }
        public Guid AssignedTo { get; set; }
        public string To { get; set; }
        public Guid AssignedBy { get; set; }
        public string By { get; set; }
        public DateTime AssignedDate { get; set; }
        public Guid AssetId { get; set; }
        public string AssetName { get; set; }
        public string AssetCode { get; set; }
        public string Note { get; set; }
        public EnumAssignmentStatus Status { get; set; }
    }
}
