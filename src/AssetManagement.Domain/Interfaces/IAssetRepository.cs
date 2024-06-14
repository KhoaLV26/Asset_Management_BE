using System;
using System.Threading.Tasks;
using AssetManagement.Domain.Entities;

namespace AssetManagement.Domain.Interfaces
{
    public interface IAssetRepository : IGenericRepository<Asset>
    {
        Task<Asset?> GetAssetDetail(Guid id);
    }
}