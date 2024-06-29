using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Domain.Entities
{
    public class ReturnRequest : BaseEntity
    {
        public Guid AssignmentId { get; set; }
        public Assignment Assignment { get; set; }
        public Guid? AcceptanceBy { get; set; }
        public User? UserAccept { get; set; }
        public DateOnly ReturnDate { get; set; }
        public EnumReturnRequestStatus ReturnStatus { get; set; } = EnumReturnRequestStatus.WaitingForReturning;
    }
}