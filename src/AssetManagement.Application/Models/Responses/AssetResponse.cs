using AssetManagement.Domain.Enums;
using System;

namespace AssetManagement.Application.Models.Responses
{
    public class AssetResponse 
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string AssetCode { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string CategoryName { get; set; }
        public EnumAssetStatus Status { get; set; } = EnumAssetStatus.Available;
    }
}
