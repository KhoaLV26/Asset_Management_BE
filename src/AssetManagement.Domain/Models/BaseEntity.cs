using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Domain.Models
{
    public class BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public Guid? CreatedBy { get; set; } = Guid.Empty;
        public DateTime? LastUpdatedAt { get; set; } = null;
        public Guid? LastUpdatedBy { get; set; } = null;
        public bool IsDeleted { get; set; } = false;
    }
}