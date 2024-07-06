using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.Services
{
    public interface ILocationService
    {
        Task<(IEnumerable<LocationResponse> data, int totalCount)> GetAllLocationAsync(int pageNumber, string? search, string? sortOrder = "asc", string? sortBy = "Name", string? newLocationCode = "", int pageSize = 10);

        Task<LocationCreateResponse> CreateLocationAsync(LocationCreateRequest request);

        Task<LocationResponse> UpdateLocationAsync(Guid locationId, LocationUpdateRequest request);

        Task<LocationResponse> GetLocationByIdAsync(Guid locationId);
    }
}