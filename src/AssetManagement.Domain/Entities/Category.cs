using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetManagement.Domain.Models;

namespace AssetManagement.Domain.Entities
{
    public class Category : BaseEntity
    {
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    }
}