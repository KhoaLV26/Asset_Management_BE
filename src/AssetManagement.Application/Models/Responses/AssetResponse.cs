using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using System;
using System.Collections.Generic;

namespace AssetManagement.Application.Models.Responses
{
    public class AssetResponse 
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string AssetCode { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public Guid CategoryId {  get; set; }
        public string CategoryName { get; set; }
        public EnumAssetStatus Status { get; set; } = EnumAssetStatus.Available;
    }
}
