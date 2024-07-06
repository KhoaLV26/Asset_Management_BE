using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.Models.Requests
{
    public class LocationCreateRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Code { get; set; } = string.Empty;
    }

    public class LocationUpdateRequest
    {
        public string? Name { get; set; } = string.Empty;
        public string? Code { get; set; } = string.Empty;
    }
}