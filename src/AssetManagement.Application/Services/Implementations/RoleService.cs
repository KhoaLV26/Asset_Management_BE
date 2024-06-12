using AssetManagement.Application.Models.Responses;
using AssetManagement.Domain.Interfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.Services.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RoleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RoleResponse>> GetAllAsync()
        {
            var roles = await _unitOfWork.RoleRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<RoleResponse>>(roles);
        }
    }
}
