using AssetManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.Models.Requests
{
    public class ReturnRequestDTO
    {
        public Guid AssignmentId { get; set; }
        public string AssetCode { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public Guid? AcceptedBy { get; set; }
        public string AcceptedByName { get; set; } = string.Empty;
        public Guid RequestedBy { get; set; }
        public string RequestedByName { get; set; } = string.Empty;
        public DateOnly AssignedDate { get; set; }
        public DateOnly ReturnDate { get; set; }
        public EnumReturnRequestStatus ReturnStatus { get; set; }
    }
}
