using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.Services
{
    public interface IRequestReturnService
    {
        Task<(IEnumerable<ReturnRequestResponse>, int totalCount)> GetReturnRequestResponses(Guid locationId, ReturnFilterRequest requestFilter);
        Task CompleteReturnRequest(Guid id);
        Task<bool> CancelRequest(Guid id);
        Task<ReturnRequestResponse> UserCreateReturnRequestAsync(Guid userId, Guid assignmentId);
    }
}