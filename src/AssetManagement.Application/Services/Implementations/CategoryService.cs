using AssetManagement.Application.Models.Responses;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(IEnumerable<CategoryResponse> data, int totalCount)> GetAllCategoryAsync(int page = 1, Expression<Func<Category, bool>>? filter = null, Func<IQueryable<Category>, IOrderedQueryable<Category>>? orderBy = null, string includeProperties = "")
        {
            var categories = await _unitOfWork.CategoryRepository.GetAllAsync(page, filter, orderBy, includeProperties);

            return (categories.items.Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name
            }), categories.totalCount);
        }
    }
}
