using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using System;
using System.Collections.Generic;

namespace AssetManagement.Application.Models.Responses
{
    public class AssetResponse
    {
        public Guid Id { get; set; }
        public string AssetCode { get; set; }
        public string AssetName { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public DateOnly InstallDate { get; set; }
        public string Specification { get; set; }
        public EnumAssetStatus Status { get; set; }
        public Guid LocationId { get; set; }
        public string LocationName { get; set; }
    }
}