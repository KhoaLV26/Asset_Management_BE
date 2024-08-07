﻿using AssetManagement.Application.Models.Requests;
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
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AssetManagement.Application.Services.Implementations
{
    public class AssignmentService : IAssignmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAssetService _assetService;

        public AssignmentService(IUnitOfWork unitOfWork, IMapper mapper, IAssetService assetService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _assetService = assetService;
        }

        public async Task<AssignmentResponse> AddAssignmentAsync(AssignmentRequest request)
        {
            var asset = await _unitOfWork.AssetRepository.GetAsync(a => a.Id == request.AssetId);
            var assignTo = await _unitOfWork.UserRepository.GetAsync(u => u.Id == request.AssignedTo);
            var assignBy = await _unitOfWork.UserRepository.GetAsync(u => u.Id == request.AssignedBy);

            if (asset == null || asset.Status != EnumAssetStatus.Available)
            {
                throw new ArgumentException("The asset is not available for assignment.");
            }

            if (asset.IsDeleted != false)
            {
                throw new ArgumentException("The asset is deleted.");
            }

            if (assignTo.IsDeleted != false || assignBy.IsDeleted != false)
            {
                throw new ArgumentException("The user is disabled.");
            }

            var assignment = new Assignment
            {
                Id = Guid.NewGuid(),
                AssignedTo = request.AssignedTo,
                AssignedBy = request.AssignedBy,
                AssignedDate = request.AssignedDate,
                AssetId = asset.Id,
                Status = EnumAssignmentStatus.WaitingForAcceptance,
                CreatedAt = DateTime.Now,
                CreatedBy = request.AssignedBy,
                Note = request.Note,
            };

            var isDeletedAsset = await _unitOfWork.AssetRepository.GetAsync(a => a.Id == assignment.AssetId);
            var isDeletedUser = await _unitOfWork.UserRepository.GetAsync(a => a.Id == assignment.AssignedTo);
            if (isDeletedAsset != null)
            {
                if (isDeletedUser != null)
                {
                    await _unitOfWork.AssignmentRepository.AddAsync(assignment);
                }
                else
                {
                    throw new ArgumentException("User does not exist");
                }
            }
            else
            {
                throw new ArgumentException("Asset does not exist");
            }
            if (await _unitOfWork.CommitAsync() < 1)
            {
                throw new ArgumentException("An error occurred while create assignment.");
            }
            else
            {
                asset.Status = EnumAssetStatus.Assigned;
                _unitOfWork.AssetRepository.Update(asset);
                if (await _unitOfWork.CommitAsync() < 1)
                {
                    throw new ArgumentException("Assignment created but failed to update asset status.");
                }
                return _mapper.Map<AssignmentResponse>(assignment);
            }
        }

        public async Task<(IEnumerable<AssignmentResponse> data, int totalCount)> GetAllAssignmentAsync(int pageNumber, string? state, DateTime? assignedDate, string? search, string? sortOrder, Guid locationId,
         string? sortBy = "assetCode", string includeProperties = "", Guid? newAssignmentId = null, int pageSize = 10)
        {
            Func<IQueryable<Assignment>, IOrderedQueryable<Assignment>>? orderBy = GetOrderQuery(sortOrder, sortBy);
            Expression<Func<Assignment, bool>> filter = await GetFilterQuery(assignedDate, state, search, locationId);
            Expression<Func<Assignment, bool>> prioritizeCondition = null;

            if (newAssignmentId.HasValue)
            {
                prioritizeCondition = u => u.Id == newAssignmentId;
            }

            var assignments = await _unitOfWork.AssignmentRepository.GetAllAsync(pageNumber, filter, orderBy, includeProperties, prioritizeCondition, pageSize);

            var assignmentResponses = assignments.items.Select(a => new AssignmentResponse
            {
                Id = a.Id,
                AssignedTo = a.AssignedTo,
                AssignedToName = a.UserTo.Username,
                AssignedBy = a.AssignedBy,
                AssignedByName = a.UserBy.Username,
                AssignedDate = a.AssignedDate,
                AssetId = a.AssetId,
                AssetCode = a.Asset.AssetCode,
                AssetName = a.Asset.AssetName,
                Specification = a.Asset.Specification,
                Note = a.Note,
                Status = a.Status,
                ReturnRequests = new ReturnRequestResponse()
            }).ToList();

            foreach (var assignmentResponse in assignmentResponses)
            {
                var returnRequestsResult = await _unitOfWork.ReturnRequestRepository.GetAllAsync(
                    page: 1,
                    filter: x => !x.IsDeleted && x.AssignmentId == assignmentResponse.Id,
                    orderBy: null,
                    "",
                    null);

                assignmentResponse.ReturnRequests = _mapper.Map<ReturnRequestResponse>(returnRequestsResult.items.FirstOrDefault());
            }

            return (assignmentResponses, assignments.totalCount);
        }

        public async Task<AssignmentResponse> GetAssignmentDetailAsync(Guid id)
        {
            var assignment = await _unitOfWork.AssignmentRepository.GetAsync(a => a.Id == id,
                     a => a.UserTo,
                     a => a.UserBy,
                     a => a.Asset);
            if (assignment == null || assignment.IsDeleted == true)
            {
                return null;
            }
            var returnRequestsResult = await _unitOfWork.ReturnRequestRepository.GetAllAsync(
                    page: 1,
                    filter: x => !x.IsDeleted && x.AssignmentId == id,
                    orderBy: null,
                    "",
                    null);

            var assignmentResponses = new AssignmentResponse
            {
                Id = assignment.Id,
                AssignedTo = assignment.AssignedTo,
                AssignedToName = assignment.UserTo.Username,
                FullName = assignment.UserTo.FirstName + " " + assignment.UserTo.LastName,
                StaffCode = assignment.UserTo.StaffCode,
                AssignedBy = assignment.AssignedBy,
                AssignedByName = assignment.UserBy.Username,
                AssignedDate = assignment.AssignedDate,
                AssetId = assignment.AssetId,
                AssetCode = assignment.Asset.AssetCode,
                AssetName = assignment.Asset.AssetName,
                Specification = assignment.Asset.Specification,
                Note = assignment.Note,
                Status = assignment.Status,
                ReturnRequests = _mapper.Map<ReturnRequestResponse>(returnRequestsResult.items.FirstOrDefault())
            };

            return (assignmentResponses);
        }

        public async Task<AssignmentResponse> UpdateAssignment(Guid id, AssignmentRequest assignmentRequest)
        {
            var currentAssignment = await _unitOfWork.AssignmentRepository.GetAsync(a => a.Id == id && !a.IsDeleted, a => a.UserTo,
                     a => a.UserBy,
                     a => a.Asset);
            if (currentAssignment == null)
            {
                throw new ArgumentException("Assignment not exist");
            }

            if (assignmentRequest.AssignedTo != Guid.Empty)
            {
                var assignedTo = await _unitOfWork.UserRepository.GetAsync(a => a.Id == assignmentRequest.AssignedTo && !a.IsDeleted);
                if (assignedTo == null)
                {
                    throw new ArgumentException("User does not exist!");
                }
                currentAssignment.AssignedTo = assignmentRequest.AssignedTo;
            }

            var oldAssignmentAsset = await _unitOfWork.AssetRepository.GetAsync(a => a.Id == currentAssignment.AssetId && !a.IsDeleted);

            if (assignmentRequest.AssetId != Guid.Empty)
            {
                var asset = await _unitOfWork.AssetRepository.GetAsync(a => a.Id == assignmentRequest.AssetId && !a.IsDeleted);
                if (asset == null)
                {
                    throw new ArgumentException("Asset does not exist!");
                }

                if (oldAssignmentAsset.Id != assignmentRequest.AssetId)
                {
                    if (asset.Status != EnumAssetStatus.Available)
                    {
                        throw new ArgumentException("Asset is not available!");
                    }
                    else
                    {
                        currentAssignment.AssetId = assignmentRequest.AssetId;
                    }
                }
            }

            currentAssignment.AssignedBy = assignmentRequest.AssignedBy == Guid.Empty ? currentAssignment.AssignedBy : assignmentRequest.AssignedBy;
            currentAssignment.AssignedDate = assignmentRequest.AssignedDate == DateTime.MinValue ? currentAssignment.AssignedDate : assignmentRequest.AssignedDate;
            currentAssignment.Status = Enum.IsDefined(typeof(EnumAssignmentStatus), assignmentRequest.Status) ? assignmentRequest.Status : currentAssignment.Status;
            currentAssignment.Note = assignmentRequest.Note;

            _unitOfWork.AssignmentRepository.Update(currentAssignment);
            if (await _unitOfWork.CommitAsync() < 1)
            {
                throw new ArgumentException("An error occurred while updating assignment.");
            }

            //Update asset status
            oldAssignmentAsset.Status = EnumAssetStatus.Available;
            _unitOfWork.AssetRepository.Update(oldAssignmentAsset);

            if (await _unitOfWork.CommitAsync() < 1)
            {
                throw new ArgumentException("The assignment was updated but failed to update old asset status.");
            }

            var currentAsset = await _unitOfWork.AssetRepository.GetAsync(a => a.Id == currentAssignment.AssetId && !a.IsDeleted);
            currentAsset.Status = EnumAssetStatus.Assigned;
            _unitOfWork.AssetRepository.Update(currentAsset);
            if (await _unitOfWork.CommitAsync() < 1)
            {
                throw new ArgumentException("The assignment was updated but failed to update new asset status.");
            }

            return new AssignmentResponse
            {
                Id = currentAssignment.Id
            };
        }

        public async Task<Expression<Func<Assignment, bool>>>? GetFilterQuery(DateTime? assignedDate, string? state, string? search, Guid locationId)
        {
            Expression<Func<Assignment, bool>>? filter = null;
            var parameter = Expression.Parameter(typeof(Assignment), "x");
            var conditions = new List<Expression>();

            if (!string.IsNullOrEmpty(state))
            {
                if (state.ToLower() != "all")
                {
                    if (Enum.TryParse<EnumAssignmentStatus>(state, true, out var parsedStatus))
                    {
                        var stateCondition = Expression.Equal(
                            Expression.Property(parameter, nameof(Assignment.Status)),
                            Expression.Constant(parsedStatus)
                        );
                        conditions.Add(stateCondition);
                    }
                    else
                    {
                        throw new InvalidCastException("Invalid status value");
                    }
                }
            }
            else
            {
                var acceptedCondition = Expression.Equal(
                    Expression.Property(parameter, nameof(Assignment.Status)),
                    Expression.Constant(EnumAssignmentStatus.Accepted)
                );

                var waitingForAcceptance = Expression.Equal(
                    Expression.Property(parameter, nameof(Assignment.Status)),
                    Expression.Constant(EnumAssignmentStatus.WaitingForAcceptance)
                );

                var defaultStateCondition = Expression.OrElse(acceptedCondition, waitingForAcceptance);

                conditions.Add(defaultStateCondition);
            }

            var isDeletedCondition = Expression.Equal(Expression.Property(parameter, nameof(Assignment.IsDeleted)), Expression.Constant(false));
            conditions.Add(isDeletedCondition);
            var locationProperty = Expression.Property(Expression.Property(parameter, nameof(Assignment.Asset)), nameof(Asset.LocationId));
            var locationCondition = Expression.Equal(
                locationProperty,
                Expression.Constant(locationId, typeof(Guid?))
            );
            conditions.Add(locationCondition);

            if (!string.IsNullOrEmpty(search))
            {
                var assetCodeProperty = Expression.Property(Expression.Property(parameter, nameof(Assignment.Asset)), nameof(Asset.AssetCode));
                var assetNameProperty = Expression.Property(Expression.Property(parameter, nameof(Assignment.Asset)), nameof(Asset.AssetName));
                var userToUsernameProperty = Expression.Property(Expression.Property(parameter, nameof(Assignment.UserTo)), nameof(User.Username));

                var searchCondition = Expression.OrElse(
                    Expression.Call(assetCodeProperty, nameof(string.Contains), Type.EmptyTypes, Expression.Constant(search)),
                    Expression.Call(assetNameProperty, nameof(string.Contains), Type.EmptyTypes, Expression.Constant(search))
                );

                var userNameCondition = Expression.Call(userToUsernameProperty, nameof(string.Contains), Type.EmptyTypes, Expression.Constant(search));

                conditions.Add(Expression.OrElse(searchCondition, userNameCondition));
            }

            if (assignedDate.HasValue)
            {
                var assignedDateValue = assignedDate.Value.Date;
                var dateProperty = Expression.Property(parameter, nameof(Assignment.AssignedDate));
                var datePropertyDate = Expression.Property(dateProperty, "Date");
                var dateCondition = Expression.Equal(datePropertyDate, Expression.Constant(assignedDateValue));
                conditions.Add(dateCondition);
            }

            if (conditions.Any())
            {
                var combinedCondition = conditions.Aggregate((left, right) => Expression.AndAlso(left, right));
                filter = Expression.Lambda<Func<Assignment, bool>>(combinedCondition, parameter);
            }

            return filter;
        }

        public Func<IQueryable<Assignment>, IOrderedQueryable<Assignment>>? GetOrderQuery(string? sortOrder, string? sortBy)
        {
            Func<IQueryable<Assignment>, IOrderedQueryable<Assignment>>? orderBy;
            switch (sortBy?.ToLower())
            {
                case SortConstants.Assignment.SORT_BY_ASSET_CODE:
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.Asset.AssetCode) : x.OrderByDescending(a => a.Asset.AssetCode);
                    break;

                case SortConstants.Assignment.SORT_BY_ASSET_NAME:
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.Asset.AssetName) : x.OrderByDescending(a => a.Asset.AssetName);
                    break;

                case SortConstants.Assignment.SORT_BY_ASSIGNED_TO:
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.UserTo.Username) : x.OrderByDescending(a => a.UserTo.Username);
                    break;

                case SortConstants.Assignment.SORT_BY_ASSIGNED_BY:
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.UserBy.Username) : x.OrderByDescending(a => a.UserBy.Username);
                    break;

                case SortConstants.Assignment.SORT_BY_ASSIGNED_DATE:
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.AssignedDate) : x.OrderByDescending(a => a.AssignedDate);
                    break;

                case SortConstants.Assignment.SORT_BY_STATE:
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.Status) : x.OrderByDescending(a => a.Status);
                    break;

                default:
                    orderBy = null;
                    break;
            }
            return orderBy;
        }

        public async Task<bool> DeleteAssignment(Guid id)
        {
            var assignment = await _unitOfWork.AssignmentRepository
                .GetAsync(a => !a.IsDeleted
                            && a.Id == id
                            && a.Status != EnumAssignmentStatus.Accepted,
                            a => a.Asset);
            if (assignment == null)
            {
                return false;
            }
            _unitOfWork.AssignmentRepository.SoftDelete(assignment);
            assignment.Asset.Status = EnumAssetStatus.Available;
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<Expression<Func<Assignment, bool>>>? GetUserFilterQuery(Guid userId)
        {
            // Determine the filtering criteria
            Expression<Func<Assignment, bool>>? filter = null;
            var parameter = Expression.Parameter(typeof(Assignment), "x");
            var conditions = new List<Expression>();

            // Condition to check if the record is not deleted
            var isDeleteCondition = Expression.Equal(Expression.Property(parameter, nameof(Assignment.IsDeleted)),
                Expression.Constant(false));
            conditions.Add(isDeleteCondition);

            // Condition for AssignedTo equals userId
            var assignedToCondition = Expression.Equal(Expression.Property(parameter, nameof(Assignment.AssignedTo)),
                Expression.Constant(userId));
            conditions.Add(assignedToCondition);

            // Condition for AssignedDate <= today
            var today = DateTime.Today;
            var dateProperty = Expression.Property(parameter, nameof(Assignment.AssignedDate));
            var dateCondition = Expression.LessThanOrEqual(dateProperty, Expression.Constant(today));
            conditions.Add(dateCondition);

            // Condition for Status not equal to Declined
            var acceptedStatus = EnumAssignmentStatus.Accepted;
            var waitingForAcceptanceStatus = EnumAssignmentStatus.WaitingForAcceptance;
            var statusProperty = Expression.Property(parameter, nameof(Assignment.Status));
            var statusCondition = Expression.OrElse(
                Expression.Equal(statusProperty, Expression.Constant(acceptedStatus)),
                Expression.Equal(statusProperty, Expression.Constant(waitingForAcceptanceStatus))
            );
            conditions.Add(statusCondition);


            // Combine all conditions
            if (conditions.Any())
            {
                var combinedCondition = conditions.Aggregate((left, right) => Expression.AndAlso(left, right));
                filter = Expression.Lambda<Func<Assignment, bool>>(combinedCondition, parameter);
            }
            return filter;
        }

        public async Task<(IEnumerable<AssignmentResponse> data, int totalCount)> GetUserAssignmentAsync(int pageNumber, Guid? newAssignmentId, Guid userId, string? sortOrder = "desc",
         string? sortBy = "assigneddate", int pageSize = 10)
        {
            Func<IQueryable<Assignment>, IOrderedQueryable<Assignment>>? orderBy = GetOrderQuery(sortOrder, sortBy);
            Expression<Func<Assignment, bool>> filter = await GetUserFilterQuery(userId);
            Expression<Func<Assignment, bool>> prioritizeCondition = null;

            if (newAssignmentId.HasValue)
            {
                prioritizeCondition = u => u.Id == newAssignmentId;
            }

            var assignments = await _unitOfWork.AssignmentRepository.GetAllAsync(pageNumber, filter, orderBy, "UserTo,UserBy,Asset,ReturnRequest", prioritizeCondition, pageSize);

            var assignmentResponses = assignments.items.Select(a => new AssignmentResponse
            {
                Id = a.Id,
                AssignedTo = a.AssignedTo,
                AssignedToName = a.UserTo.Username,
                AssignedBy = a.AssignedBy,
                AssignedByName = a.UserBy.Username,
                AssignedDate = a.AssignedDate,
                AssetId = a.AssetId,
                AssetCode = a.Asset.AssetCode,
                AssetName = a.Asset.AssetName,
                Specification = a.Asset.Specification,
                Note = a.Note,
                Status = a.Status,
                ReturnRequests = new ReturnRequestResponse()
            }).ToList();

            foreach (var assignmentResponse in assignmentResponses)
            {
                var returnRequestsResult = await _unitOfWork.ReturnRequestRepository.GetAllAsync(
                    page: 1,
                    filter: x => !x.IsDeleted && x.AssignmentId == assignmentResponse.Id,
                    orderBy: null,
                    "",
                    null);

                assignmentResponse.ReturnRequests = _mapper.Map<ReturnRequestResponse>(returnRequestsResult.items.FirstOrDefault());
            }

            return (assignmentResponses, assignments.totalCount);
        }

        public async Task<AssignmentResponse> ResponseAssignment(Guid id, Guid userId, string accepted)
        {
            var currentAssignment = await _unitOfWork.AssignmentRepository.GetAsync(a => a.Id == id && !a.IsDeleted, a => a.UserTo,
                     a => a.UserBy,
                     a => a.Asset);
            if (currentAssignment == null)
            {
                throw new ArgumentException("Assignment not exist");
            }
            var assignedTo = await _unitOfWork.UserRepository.GetAsync(a => a.Id == currentAssignment.AssignedTo && !a.IsDeleted);
            if (assignedTo == null)
            {
                throw new ArgumentException("User does not exist!");
            }

            if (userId != currentAssignment.AssignedTo)
            {
                throw new ArgumentException("This is not your assignment!");
            }

            var asset = await _unitOfWork.AssetRepository.GetAsync(a => a.Id == currentAssignment.AssetId && !a.IsDeleted);
            if (asset == null)
            {
                throw new ArgumentException("Asset does not exist!");
            }

            if (asset.Status != EnumAssetStatus.Assigned)
            {
                throw new ArgumentException("Asset is not assigned!");
            }

            currentAssignment.Status = (accepted.ToLower() == "true") ? EnumAssignmentStatus.Accepted : EnumAssignmentStatus.Declined;
            _unitOfWork.AssignmentRepository.Update(currentAssignment);
            if (await _unitOfWork.CommitAsync() < 1)
            {
                throw new ArgumentException("An error occurred while response to assignment!");
            }

            return new AssignmentResponse
            {
                Id = currentAssignment.Id
            };
        }
    }
}
