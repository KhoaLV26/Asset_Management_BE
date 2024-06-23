using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.Services
{
    public interface IUserService
    {
        Task<UserRegisterResponse> AddUserAsync(UserRegisterRequest userRegisterRequest);
        Task<Guid> GetLocation(Guid id);
        Task<(IEnumerable<GetUserResponse> Items, int TotalCount)> GetFilteredUsersAsync(
        string adminId,
        string? searchTerm,
        string? role = null,
        string sortBy = "StaffCode",
        string sortDirection = "asc",
        int pageNumber = 1,
        string? newStaffCode = "");

        Task<bool> DisableUser(Guid id);

    }
}