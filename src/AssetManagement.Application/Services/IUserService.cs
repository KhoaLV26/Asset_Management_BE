using AssetManagement.Application.Models.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.Services
{
    public interface IUserService
    {
        Task AddUserAsync(UserRegisterRequest userRegisterRequest);
    }
}
