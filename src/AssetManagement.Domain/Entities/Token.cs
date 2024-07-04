using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Domain.Entities
{
    public class Token
    {
        [Key]
        public long Id { get; set; }

        public Guid UserId { get; set; }
        public User? User { get; set; }
        public string HashToken { get; set; } = string.Empty;
    }
}