using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.Models.Responses
{
    public class ReturnRequestResponse
    {
        public Guid Id { get; set; }
        public Guid AssignmentId { get; set; }
        public string AssetCode { get; set; }
        public string AssetName { get; set; }
        public Guid AcceptanceBy { get; set; }
        public string AcceptanceByName { get; set; }
        public Guid RequestedBy { get; set; }
        public string RequestedByName { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateOnly ReturnDate { get; set; }
        public string ReturnStatus { get; set; } = EnumReturnRequestStatus.WaitingForReturning.ToString();
    }
}