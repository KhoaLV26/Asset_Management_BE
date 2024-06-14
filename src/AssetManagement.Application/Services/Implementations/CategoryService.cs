using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Interfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetManagement.Application.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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

        public async Task<CategoryResponse> CreateCategoryAsync(CategoryRequest categoryRequest)
        {
            var categoryNameExisted = await _unitOfWork.CategoryRepository.GetAsync(c => c.Name == categoryRequest.Name);
            if (categoryNameExisted != null)
            {
                throw new ArgumentException("Category is already existed. Please enter a different category");
            }

            var categoryCodeExisted = await _unitOfWork.CategoryRepository.GetAsync(c => c.Code == categoryRequest.Code);
            if (categoryCodeExisted != null)
            {
                throw new ArgumentException("Prefix is already existed. Please enter a different prefix");
            }

            var newCategory = new Category
            {
                Name = categoryRequest.Name,
                Code = categoryRequest.Code.ToUpper(),
                CreatedBy = categoryRequest.CreatedBy,
            };

            await _unitOfWork.CategoryRepository.AddAsync(newCategory);
            if (await _unitOfWork.CommitAsync() > 0)
            {
                return _mapper.Map<CategoryResponse>(newCategory);
            }
            else
            {
                throw new Exception("Failed to create category");
            }
        }
    }
}