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
    }
}
