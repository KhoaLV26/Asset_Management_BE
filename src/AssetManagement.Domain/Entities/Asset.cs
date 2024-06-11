using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Domain.Entities
{
    public class Asset : BaseEntity
    {
        [MaxLength(10)]
        public string AssetCode { get; set; } = string.Empty;

        [MaxLength(100)]
        public string AssetName { get; set; } = string.Empty;

        public Guid CategoryId { get; set; }
        public Category Category { get; set; }
        public EnumAssetStatus Status { get; set; } = EnumAssetStatus.Available;
        public DateOnly InstallDate { get; set; }
        public Guid AssignmentId { get; set; }
        public Assignment Assignment { get; set; }

        [MaxLength(255)]
        public string Specification { get; set; } = string.Empty;
    }
}