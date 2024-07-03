using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Interfaces;
using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.Services.Implementations
{
    public class RequestReturnService : IRequestReturnService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RequestReturnService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<(IEnumerable<ReturnRequestResponse>, int totalCount)> GetReturnRequestResponses(Guid locationId, ReturnFilterRequest requestFilter)
        {
            Func<IQueryable<ReturnRequest>, IOrderedQueryable<ReturnRequest>> orderBy = null;

            bool ascending = requestFilter.SortOrder.ToLower() == "asc";

            switch (requestFilter.SortBy)
            {
                case "AssetName":
                    orderBy = q => ascending ? q.OrderBy(u => u.Assignment.Asset.AssetName) : q.OrderByDescending(u => u.Assignment.Asset.AssetName);
                    break;

                case "RequestedBy":
                    orderBy = q => ascending ? q.OrderBy(u => u.Assignment.UserTo.Username) : q.OrderByDescending(u => u.Assignment.UserTo.Username);
                    break;

                case "AssignedDate":
                    orderBy = q => ascending ? q.OrderBy(u => u.Assignment.AssignedDate) : q.OrderByDescending(u => u.Assignment.AssignedDate);
                    break;

                case "AcceptedBy":
                    orderBy = q => ascending ? q.OrderBy(u => u.UserAccept.Username) : q.OrderByDescending(u => u.UserAccept.Username);
                    break;

                case "ReturnedDate":
                    orderBy = q => ascending ? q.OrderBy(u => u.ReturnDate) : q.OrderByDescending(u => u.ReturnDate);
                    break;

                case "State":
                    orderBy = q => ascending ? q.OrderBy(u => u.ReturnStatus) : q.OrderByDescending(u => u.ReturnStatus);
                    break;

                default:
                    orderBy = q => ascending ? q.OrderBy(u => u.Assignment.Asset.AssetCode) : q.OrderByDescending(u => u.Assignment.Asset.AssetCode);
                    break;
            }
            var returnRequests = await _unitOfWork.ReturnRequestRepository
                .GetAllAsync(requestFilter.PageNumber.Value,
                            x => x.Assignment.UserBy.LocationId == locationId &&
                            (!requestFilter.Status.HasValue || requestFilter.Status == 0 || (requestFilter.Status != 0 && (int)x.ReturnStatus == requestFilter.Status.Value)) &&
                            (!requestFilter.ReturnDate.HasValue || x.ReturnDate == requestFilter.ReturnDate.Value) &&
                            (string.IsNullOrEmpty(requestFilter.SearchTerm) || x.Assignment.Asset.AssetCode.Contains(requestFilter.SearchTerm) ||
                            x.Assignment.Asset.AssetName.Contains(requestFilter.SearchTerm) || x.Assignment.UserTo.Username.Contains(requestFilter.SearchTerm)),
                            orderBy,
                            "Assignment,Assignment.Asset,UserAccept,Assignment.UserTo",
                            null);

            return (_mapper.Map<IEnumerable<ReturnRequestResponse>>(returnRequests.items), returnRequests.totalCount);
        }

        public async Task<bool> CancelRequest(Guid id)
        {
            var request = await _unitOfWork.ReturnRequestRepository.GetAsync(r => r.Id == id);
            if (request == null)
            {
                throw new ArgumentException("Request not found.");
            }

            if (request.ReturnStatus == EnumReturnRequestStatus.Completed)
            {
                throw new ArgumentException("Can't cancel request already completed");
            }

            var assignment = await _unitOfWork.AssignmentRepository.GetAsync(a => a.Id == request.AssignmentId);
            if (assignment == null)
            {
                throw new ArgumentException("Associated assignment not found.");
            }

            request.IsDeleted = true;
            _unitOfWork.ReturnRequestRepository.Update(request);

            assignment.Status = EnumAssignmentStatus.Accepted;
            _unitOfWork.AssignmentRepository.Update(assignment);

            var result = await _unitOfWork.CommitAsync();
            return result > 0;
        }
    }
}