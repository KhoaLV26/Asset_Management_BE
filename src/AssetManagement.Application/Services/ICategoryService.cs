using AssetManagement.Application.Models.Responses;
using AssetManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.Services
{
    internal interface ICategoryService
    {
        Task<(IEnumerable<CategoryResponse> data, int totalCount)> GetAllCategoryAsync(int page = 1, Expression<Func<Category, bool>>? filter = null, Func<IQueryable<Category>, IOrderedQueryable<Category>>? orderBy = null, string includeProperties = "");

    }
}
