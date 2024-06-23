using AssetManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.Models.Responses
{
    public class AssetDetailResponse
    {
        public Guid Id { get; set; }
        public string AssetCode { get; set; }
        public string AssetName { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public Guid LocationId { get; set; }
        public EnumAssetStatus Status { get; set; }
        public IEnumerable<AssignmentResponse>? AssignmentResponses { get; set; }
    }
}