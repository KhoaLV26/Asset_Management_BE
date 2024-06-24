using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Interfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.Services.Implementations
{
    public class AssignmentService : IAssignmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        
        public AssignmentService (IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<AssignmentResponse> AddAssignmentAsync(AssignmentRequest request)
        {
            var asset = await _unitOfWork.AssetRepository.GetAsync(a => a.Id == request.AssetId);
            if (asset == null || asset.Status != EnumAssetStatus.Available)
            {
                throw new InvalidOperationException("The asset is not available for assignment.");
            }
            var assignment = new Assignment
            {
                Id = Guid.NewGuid(),
                AssignedTo = request.AssignedTo,
                AssignedBy = request.AssignedBy,
                AssignedDate = request.AssignedDate,
                AssetId = asset.Id,
                Status = EnumAssignmentStatus.WaitingForAcceptance,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.AssignedBy,
                Note = request.Note,
            };
            await _unitOfWork.AssignmentRepository.AddAsync(assignment);
            if (await _unitOfWork.CommitAsync() < 1)
            {
                throw new InvalidOperationException("An error occurred while create assignment.");
            }
            else
            {
                asset.Status = EnumAssetStatus.NotAvailable;
                _unitOfWork.AssetRepository.Update(asset);
                return _mapper.Map<AssignmentResponse>(assignment);
            }
        }
    }
}
