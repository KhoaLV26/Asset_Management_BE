using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.Services
{
    public interface IAssignmentService
    {
        Task<AssignmentResponse> AddAssignmentAsync(AssignmentRequest request);

        Task<(IEnumerable<AssignmentResponse> data, int totalCount)> GetAllAssignmentAsync(int pageNumber, string? state, DateTime? assignedDate, string? search, string? sortOrder, string? sortBy = "assetCode", string includeProperties = "", Guid? newAssignmentId = null);

        Task<AssignmentResponse> GetAssignmentDetailAsync(Guid id);

        Task<bool> DeleteAssignment(Guid id);
    }
}