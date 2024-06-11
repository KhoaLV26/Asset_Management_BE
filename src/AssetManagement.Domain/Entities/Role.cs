using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AssetManagement.Domain.Models;

namespace AssetManagement.Domain.Entities
{
    public class Role : BaseEntity
    {
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}