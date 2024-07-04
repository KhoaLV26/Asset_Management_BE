using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Domain.Entities
{
    public class BlackListToken
    {
        [Key]
        public long Id { get; set; }

        public string Token { get; set; } = string.Empty;
    }
}