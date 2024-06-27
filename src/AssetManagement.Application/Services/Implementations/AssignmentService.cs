using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.Interfaces;
using AutoMapper;
using MailKit.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.Services.Implementations
{
    public class AssignmentService : IAssignmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AssignmentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AssignmentResponse> AddAssignmentAsync(AssignmentRequest request)
        {
            var asset = await _unitOfWork.AssetRepository.GetAsync(a => a.Id == request.AssetId);
            if (asset == null || asset.Status != EnumAssetStatus.Available)
            {
                throw new ArgumentException("The asset is not available for assignment.");
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
                throw new ArgumentException("An error occurred while create assignment.");
            }
            else
            {
                asset.Status = EnumAssetStatus.Assigned;
                _unitOfWork.AssetRepository.Update(asset);
                // Commit the asset status change
                if (await _unitOfWork.CommitAsync() < 1)
                {
                    throw new ArgumentException("Assignment created but failed to update asset status.");
                }
                return _mapper.Map<AssignmentResponse>(assignment);
            }
        }

        public async Task<(IEnumerable<AssignmentResponse> data, int totalCount)> GetAllAssignmentAsync(int pageNumber, string? state, DateTime? assignedDate, string? search, string? sortOrder,
         string? sortBy = "assetCode", string includeProperties = "", Guid? newAssignmentId = null)
        {
            Func<IQueryable<Assignment>, IOrderedQueryable<Assignment>>? orderBy = GetOrderQuery(sortOrder, sortBy);
            Expression<Func<Assignment, bool>> filter = await GetFilterQuery(assignedDate, state, search);
            Expression<Func<Assignment, bool>> prioritizeCondition = null;

            if (newAssignmentId.HasValue)
            {
                prioritizeCondition = u => u.Id == newAssignmentId;
            }

            var includes = "UserTo,UserBy,Asset";
            if (!string.IsNullOrEmpty(includeProperties))
            {
                includes = $"{includes},{includeProperties}";
            }

            var assignments = await _unitOfWork.AssignmentRepository.GetAllAsync(pageNumber, filter, orderBy, includes, prioritizeCondition);

            return (assignments.items.Select(a => new AssignmentResponse
            {
                Id = a.Id,
                AssignedTo = a.AssignedTo,
                To = a.UserTo.Username,
                AssignedBy = a.AssignedBy,
                By = a.UserBy.Username,
                AssignedDate = a.AssignedDate,
                AssetId = a.AssetId,
                AssetCode = a.Asset.AssetCode,
                AssetName = a.Asset.AssetName,
                Note = a.Note,
                Status = a.Status
            }), assignments.totalCount);
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
            return new AssignmentResponse
            {
                Id = assignment.Id,
                AssignedTo = assignment.AssignedTo,
                To = assignment.UserTo.Username,
                AssignedBy = assignment.AssignedBy,
                By = assignment.UserBy.Username,
                AssignedDate = assignment.AssignedDate,
                AssetId = assignment.AssetId,
                AssetCode = assignment.Asset.AssetCode,
                AssetName = assignment.Asset.AssetName,
                Note = assignment.Note,
                Status = assignment.Status
            };
        }

        public async Task<bool> UpdateAssignment(Guid id, AssignmentRequest assignmentRequest)
        {
            var currentAssignment = await _unitOfWork.AssignmentRepository.GetAsync(x => x.Id == id);
            if (currentAssignment == null)
            {
                return false;
            }
            if (assignmentRequest.AssignedTo != Guid.Empty)
            {
                currentAssignment.AssignedTo = assignmentRequest.AssignedTo;
            }

            if (assignmentRequest.AssignedBy != Guid.Empty)
            {
                currentAssignment.AssignedBy = assignmentRequest.AssignedBy;
            }

            if (assignmentRequest.AssignedDate == DateTime.MinValue)
            {
                currentAssignment.AssignedDate = assignmentRequest.AssignedDate;
            }

            if (assignmentRequest.AssetId != Guid.Empty)
            {
                currentAssignment.AssetId = assignmentRequest.AssetId;
            }

            if (Enum.IsDefined(typeof(EnumAssignmentStatus), assignmentRequest.Status))
            {
                currentAssignment.Status = assignmentRequest.Status;
            }

            currentAssignment.Note = assignmentRequest.Note;

            _unitOfWork.AssignmentRepository.Update(currentAssignment);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<Expression<Func<Assignment, bool>>>? GetFilterQuery(DateTime? assignedDate, string? state, string? search)
        {
            Expression<Func<Assignment, bool>>? filter = null;
            var parameter = Expression.Parameter(typeof(Assignment), "x");
            var conditions = new List<Expression>();

            // Parse state parameter to enum
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
                // Default states: Accepted, Waiting for acceptance
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

            // Add search conditions
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

            // Add date conditions
            if (assignedDate.HasValue)
            {
                var assignedDateValue = assignedDate.Value.Date;
                var dateProperty = Expression.Property(parameter, nameof(Assignment.AssignedDate));
                var datePropertyDate = Expression.Property(dateProperty, "Date");
                var dateCondition = Expression.Equal(datePropertyDate, Expression.Constant(assignedDateValue));
                conditions.Add(dateCondition);
            }

            // Combine all conditions with AndAlso
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
                case "assetcode":
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.Asset.AssetCode) : x.OrderByDescending(a => a.Asset.AssetCode);
                    break;

                case "assetname":
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.Asset.AssetName) : x.OrderByDescending(a => a.Asset.AssetName);
                    break;

                case "assignedto":
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.UserTo.Username) : x.OrderByDescending(a => a.UserTo.Username);
                    break;

                case "assignedby":
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.UserBy.Username) : x.OrderByDescending(a => a.UserBy.Username);
                    break;

                case "assigneddate":
                    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.AssignedDate) : x.OrderByDescending(a => a.AssignedDate);
                    break;

                case "state":
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

        public async Task<(IEnumerable<AssignmentResponse> data, int totalCount)> GetUserAssignmentAsync(int pageNumber, Guid userId, string? sortOrder = "desc",
         string? sortBy = "assigneddate")
        {
            Func<IQueryable<Assignment>, IOrderedQueryable<Assignment>>? orderBy = GetOrderQuery(sortOrder, sortBy);
            Expression<Func<Assignment, bool>> filter = await GetUserFilterQuery(userId);
            var assignments = await _unitOfWork.AssignmentRepository.GetAllAsync(pageNumber, filter, orderBy, "UserTo,UserBy,Asset");

            return (assignments.items.Select(a => new AssignmentResponse
            {
                Id = a.Id,
                AssignedTo = a.AssignedTo,
                To = a.UserTo.Username,
                AssignedBy = a.AssignedBy,
                By = a.UserBy.Username,
                AssignedDate = a.AssignedDate,
                AssetId = a.AssetId,
                AssetCode = a.Asset.AssetCode,
                AssetName = a.Asset.AssetName,
                Note = a.Note,
                Status = a.Status
            }), assignments.totalCount);
        }
    }
}
