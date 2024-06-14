using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.Models.Responses
{
    public class AssetRequest
    {
        public Guid CreatedBy { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public string Specification { get; set; } = string.Empty;
        public DateOnly InstallDate { get; set; }
        public EnumAssetStatus Status { get; set; } = EnumAssetStatus.Available;
    }
}