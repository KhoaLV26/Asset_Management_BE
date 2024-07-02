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
        Task<ReturnRequestResponse> AddReturnRequestAsync(Guid assignmentId);
        Task<(IEnumerable<ReturnRequestResponse>, int totalCount)> GetReturnRequestResponses(Guid locationId, ReturnFilterRequest requestFilter);
        Task CompleteReturnRequest(Guid id, Guid userId);
        Task<bool> CancelRequest(Guid id);
    }
}