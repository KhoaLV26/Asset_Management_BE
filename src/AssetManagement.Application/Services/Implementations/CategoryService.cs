using AssetManagement.Application.Models.Responses;
using AssetManagement.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync()
        {
            var categories = await _unitOfWork.CategoryRepository.GetAllAsync(c => !c.IsDeleted);
            return categories.Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code
            });
        }
    }
}
