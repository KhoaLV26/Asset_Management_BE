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
            var assignment = new Assignment
            {
                Id = Guid.NewGuid(),
                AssignedTo = request.AssignedTo,
                AssignedBy = request.AssignedBy,
                AssignedDate = request.AssignedDate,
                AssetId = request.AssetId,
                Status = EnumAssignmentStatus.WaitingForAcceptance,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.AssignedBy,
                Note = request.Note,
            };
            await _unitOfWork.AssignmentRepository.AddAsync(assignment);
            if (await _unitOfWork.CommitAsync() < 1)
            {
                throw new InvalidOperationException("An error occurred while registering the user.");
            }
            else
            {
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

            var assignments = await _unitOfWork.AssignmentRepository.GetAllAsync(pageNumber, filter, orderBy, includeProperties,
                prioritizeCondition);

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
            var assignment = await _unitOfWork.AssignmentRepository.GetAssignmentDetailAsync(id);
            if (assignment == null)
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

        private async Task<Expression<Func<Assignment, bool>>>? GetFilterQuery(DateTime? assignedDate, string? state, string? search)
        {
            // Determine the filtering criteria
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
            //else
            //{
            //    // Default states: Accepted, Waiting for acceptance
            //    var acceptedCondition = Expression.Equal(
            //        Expression.Property(parameter, nameof(Assignment.Status)),
            //        Expression.Constant(EnumAssignmentStatus.Accepted)
            //    );

            //    var waitingForAcceptance = Expression.Equal(
            //        Expression.Property(parameter, nameof(Assignment.Status)),
            //        Expression.Constant(EnumAssignmentStatus.WaitingForAcceptance)
            //    );

            //    var defaultStateCondition = Expression.OrElse(
            //        Expression.OrElse(acceptedCondition, waitingForAcceptance),
            //        waitingForAcceptance
            //    );

            //    conditions.Add(defaultStateCondition);
            //}
            // Add search conditions
            if (!string.IsNullOrEmpty(search))
            {
                var assetProperty = Expression.Property(parameter, nameof(Assignment.Asset));
                var assetCodeProperty = Expression.Property(assetProperty, nameof(Asset.AssetCode));
                var assetNameProperty = Expression.Property(assetProperty, nameof(Asset.AssetName));
                var userToProperty = Expression.Property(parameter, nameof(Assignment.UserTo));
                var usernameProperty = Expression.Property(userToProperty, nameof(User.Username));

                var searchCondition = Expression.OrElse(
                    Expression.Call(assetNameProperty, nameof(string.Contains), Type.EmptyTypes, Expression.Constant(search)),
                    Expression.OrElse(
                        Expression.Call(assetCodeProperty, nameof(string.Contains), Type.EmptyTypes, Expression.Constant(search)),
                        Expression.Call(usernameProperty, nameof(string.Contains), Type.EmptyTypes, Expression.Constant(search))
                    )       
            );
                conditions.Add(searchCondition);
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

        private Func<IQueryable<Assignment>, IOrderedQueryable<Assignment>>? GetOrderQuery(string? sortOrder, string? sortBy)
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

    }
}
