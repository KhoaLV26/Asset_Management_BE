﻿using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Interfaces;
using AutoMapper;
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

        public async Task<ReturnRequestResponse> AddReturnRequestAsync(Guid assignmentId)
        {
            var assignment = await _unitOfWork.AssignmentRepository.GetAsync(a => a.Id == assignmentId);
            if (assignment == null || assignment.Status != EnumAssignmentStatus.Accepted)
            {
                throw new ArgumentException("The assignment is not available for return request.");
            }

            var returnRequest = new ReturnRequest
            {
                Id = Guid.NewGuid(),
                AssignmentId = assignmentId,
                ReturnDate = DateOnly.FromDateTime(DateTime.Now),
                ReturnStatus = EnumReturnRequestStatus.WaitingForReturning
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

        public async Task CompleteReturnRequest(Guid id)
        {
            var returnRequest = await _unitOfWork.ReturnRequestRepository.GetAsync(x => x.Id == id,includeProperties: a => a.Assignment);
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
            var request = await _unitOfWork.ReturnRequestRepository.GetAsync(r => r.Id == id);
            if (request == null)
            {
                throw new ArgumentException("Request not found.");
            }

            if (request.ReturnStatus == EnumReturnRequestStatus.Completed)
            {
                throw new ArgumentException("Can't cancel request already completed");
            }

            request.IsDeleted = true;
            _unitOfWork.ReturnRequestRepository.Update(request);

            var result = await _unitOfWork.CommitAsync();
            return result > 0;
        }
    }
}