using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.Models.Responses
{
    public class RoleResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
