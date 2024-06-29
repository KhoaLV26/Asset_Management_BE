using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.Models.Requests
{
    public class ReturnFilterRequest
    {
        public int? PageNumber { get; set; } = 1;
        public string? SearchTerm { get; set; } = string.Empty;
        public string? SortBy { get; set; } = "AssetCode";
        public string? SortOrder { get; set; } = "asc";
        public int? Status { get; set; } = 0;
        public DateOnly? ReturnDate { get; set; }
    }
}