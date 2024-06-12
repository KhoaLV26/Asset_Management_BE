using AssetManagement.Application.Models.Requests;
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
        Task <User> AddUserAsync(UserRegisterRequest userRegisterRequest);
    }
}
