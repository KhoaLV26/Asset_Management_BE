using AssetManagement.Domain.Enums;
using System;
using System.Collections.Generic;

namespace AssetManagement.Application.Models.Responses
{
    public class AssignmentResponse
    {
        public Guid Id { get; set; }
        public Guid AssignedTo { get; set; }
        public string AssignedToName { get; set; }
        public string StaffCode { get; set; }
        public string FullName { get; set; }
        public Guid AssignedBy { get; set; }
        public string AssignedByName { get; set; }
        public DateTime AssignedDate { get; set; }
        public Guid AssetId { get; set; }
        public string AssetName { get; set; }
        public string AssetCode { get; set; }
        public string Specification { get; set; }
        public string Note { get; set; }
        public EnumAssignmentStatus Status { get; set; }
        public ReturnRequestResponse? ReturnRequests { get; set; }
    }
}
