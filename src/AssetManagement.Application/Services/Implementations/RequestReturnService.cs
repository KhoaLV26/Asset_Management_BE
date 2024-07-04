using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Domain.Constants;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<ReturnRequestResponse> UserCreateReturnRequestAsync(Guid assignmentId, Guid userId)
        {
            //Fix: Add a check for assignment.IsDeleted
            var assignment = await _unitOfWork.AssignmentRepository.GetAsync(a => a.Id == assignmentId && a.IsDeleted == false); 

            if (assignment == null || assignment.IsDeleted == true)
            {
                throw new ArgumentException("Assignment not found!");
            }

            if (assignment.Status != EnumAssignmentStatus.Accepted)
            {
                throw new ArgumentException("Invalid assignment!");
            }

            if (assignment.AssignedTo != userId)
            {
                throw new ArgumentException("Not your assignment!");
            }

            var returnRequests = await _unitOfWork.ReturnRequestRepository.GetAllAsync(
                page: 1,
                filter: x => !x.IsDeleted && x.AssignmentId == assignmentId,
                orderBy: null,
                "",
                null);
            if (returnRequests.totalCount > 0)
            {
                throw new ArgumentException("A return request for this assignment already exists.");
            }

            var returnRequest = new ReturnRequest
            {
                AssignmentId = assignmentId,
                Assignment = assignment,
                ReturnStatus = EnumReturnRequestStatus.WaitingForReturning,
                CreatedBy = userId
            };

            var existedReturnRequest = await _unitOfWork.ReturnRequestRepository.GetAsync(a => a.AssignmentId == assignmentId);
            if (existedReturnRequest == null)
            {
                _unitOfWork.ReturnRequestRepository.AddAsync(returnRequest);
            } else
            {
                throw new Exception("Return request already existed!");
            }
            if (await _unitOfWork.CommitAsync() < 1)
            {
                throw new ArgumentException("An error occurred while create return request.");
            }
            return _mapper.Map<ReturnRequestResponse>(returnRequest);
        }

        public async Task<ReturnRequestResponse> AddReturnRequestAsync(Guid adminId, Guid assignmentId)
        {
            var assignment = await _unitOfWork.AssignmentRepository.GetAsync(a => a.Id == assignmentId && !a.IsDeleted);
            if (assignment == null || assignment.Status != EnumAssignmentStatus.Accepted)
            {
                throw new ArgumentException("The assignment is not available for return request.");
            }

            var returnRequests = await _unitOfWork.ReturnRequestRepository.GetAllAsync(
                    page: 1,
                    filter: x => !x.IsDeleted && x.AssignmentId == assignmentId,
                    orderBy: null,
                    "",
                    null);
            if (returnRequests.totalCount > 0)
            {
                throw new ArgumentException("A return request for this assignment already exists.");
            }

            var returnRequest = new ReturnRequest
            {
                Id = Guid.NewGuid(),
                AssignmentId = assignmentId,
                ReturnStatus = EnumReturnRequestStatus.WaitingForReturning,
                CreatedBy = adminId
            };
            await _unitOfWork.ReturnRequestRepository.AddAsync(returnRequest);
            if (await _unitOfWork.CommitAsync() < 1)
            {
                throw new ArgumentException("An error occurred while create return request.");
            }
            else
            {
                return _mapper.Map<ReturnRequestResponse>(returnRequest);
            }
        }

        public async Task<(IEnumerable<ReturnRequestResponse>, int totalCount)> GetReturnRequestResponses(Guid locationId, ReturnFilterRequest requestFilter)
        {
            Func<IQueryable<ReturnRequest>, IOrderedQueryable<ReturnRequest>> orderBy = null;

            bool ascending = requestFilter.SortOrder.ToLower() == "asc";

            switch (requestFilter.SortBy)
            {
                case SortConstants.RequestReturn.SORT_BY_ASSET_NAME:
                    orderBy = q => ascending ? q.OrderBy(u => u.Assignment.Asset.AssetName) : q.OrderByDescending(u => u.Assignment.Asset.AssetName);
                    break;

                case SortConstants.RequestReturn.SORT_BY_REQUESTED_BY:
                    orderBy = q => ascending ? q.OrderBy(u => u.Assignment.UserTo.Username) : q.OrderByDescending(u => u.Assignment.UserTo.Username);
                    break;

                case SortConstants.RequestReturn.SORT_BY_ASSIGNED_DATE:
                    orderBy = q => ascending ? q.OrderBy(u => u.Assignment.AssignedDate) : q.OrderByDescending(u => u.Assignment.AssignedDate);
                    break;

                case SortConstants.RequestReturn.SORT_BY_ACCEPTED_BY:
                    orderBy = q => ascending ? q.OrderBy(u => u.UserAccept.Username) : q.OrderByDescending(u => u.UserAccept.Username);
                    break;

                case SortConstants.RequestReturn.SORT_BY_RETURNED_DATE:
                    orderBy = q => ascending ? q.OrderBy(u => u.ReturnDate) : q.OrderByDescending(u => u.ReturnDate);
                    break;

                case SortConstants.RequestReturn.SORT_BY_STATE:
                    orderBy = q => ascending ? q.OrderBy(u => u.ReturnStatus) : q.OrderByDescending(u => u.ReturnStatus);
                    break;

                default:
                    orderBy = q => ascending ? q.OrderBy(u => u.Assignment.Asset.AssetCode) : q.OrderByDescending(u => u.Assignment.Asset.AssetCode);
                    break;
            }
            var returnRequests = await _unitOfWork.ReturnRequestRepository
                .GetAllAsync(requestFilter.PageNumber.Value,
                            x => !x.IsDeleted && x.Assignment.UserBy.LocationId == locationId &&
                            (!requestFilter.Status.HasValue || requestFilter.Status == 0 || (requestFilter.Status != 0 && (int)x.ReturnStatus == requestFilter.Status.Value)) &&
                            (!requestFilter.ReturnDate.HasValue || x.ReturnDate == requestFilter.ReturnDate.Value) &&
                            (string.IsNullOrEmpty(requestFilter.SearchTerm) || x.Assignment.Asset.AssetCode.Contains(requestFilter.SearchTerm) ||
                            x.Assignment.Asset.AssetName.Contains(requestFilter.SearchTerm) || x.Assignment.UserTo.Username.Contains(requestFilter.SearchTerm)),
                            orderBy,
                            "Assignment,Assignment.Asset,UserAccept,Assignment.UserTo",
                            null);

            return (_mapper.Map<IEnumerable<ReturnRequestResponse>>(returnRequests.items), returnRequests.totalCount);
        }

        public async Task CompleteReturnRequest(Guid id, Guid userId)
        {
            var returnRequest = await _unitOfWork.ReturnRequestRepository.GetAsync(x => x.Id == id && !x.IsDeleted, includeProperties: a => a.Assignment);
            if (returnRequest == null)
            {
                throw new ArgumentException("Return request not exist");
            }
            if (returnRequest.ReturnStatus != EnumReturnRequestStatus.WaitingForReturning)
            {
                throw new ArgumentException("The request's status is not valid to complete");
            }
            returnRequest.ReturnStatus = EnumReturnRequestStatus.Completed;
            returnRequest.ReturnDate = DateOnly.FromDateTime(DateTime.Now);
            returnRequest.AcceptanceBy = userId;
            var asset = await _unitOfWork.AssetRepository.GetAsync(x => x.Id == returnRequest.Assignment.AssetId);
            if (asset == null)
            {
                throw new ArgumentException("Asset not exist");
            }

            var assignment = await _unitOfWork.AssignmentRepository.GetAsync(x => x.Id == returnRequest.AssignmentId);
            if (assignment == null)
            {
                throw new ArgumentException("Assignment not exist");
            }

            asset.Status = EnumAssetStatus.Available;
            assignment.Status = EnumAssignmentStatus.Returned;
            _unitOfWork.AssetRepository.Update(asset);
            _unitOfWork.AssignmentRepository.Update(assignment);
            await _unitOfWork.CommitAsync();
        }

        public async Task<bool> CancelRequest(Guid id)
        {
            var request = await _unitOfWork.ReturnRequestRepository
                .GetAsync(r => !r.IsDeleted
                            && r.Id == id
                            && r.ReturnStatus != EnumReturnRequestStatus.Completed,
                            r => r.Assignment);

            if (request == null)
            {
                throw new ArgumentException("Request not found.");
            }

            _unitOfWork.ReturnRequestRepository.SoftDelete(request);
            request.Assignment.Status = EnumAssignmentStatus.Accepted;

            var result = await _unitOfWork.CommitAsync();
            return result > 0;
        }
    }
}