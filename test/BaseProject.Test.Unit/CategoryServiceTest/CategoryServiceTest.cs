using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Services.Implementations;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Interfaces;
using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.CategoryServiceTest
{
    public class CategoryServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly CategoryService _categoryService;

        public CategoryServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _categoryService = new CategoryService(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ShouldReturnCategories()
        {
            // Arrange
            var categories = new List<Category>
    {
        new Category { Id = Guid.NewGuid(), Name = "Electronics", Code = "ELEC", IsDeleted = false },
        new Category { Id = Guid.NewGuid(), Name = "Furniture", Code = "FURN", IsDeleted = false }
    };

            var expectedCategories = categories.Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code
            });

            _mockUnitOfWork.Setup(u => u.CategoryRepository.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
                .ReturnsAsync(categories);

            _mockMapper.Setup(m => m.Map<IEnumerable<CategoryResponse>>(It.IsAny<IEnumerable<Category>>()))
                .Returns(categories.Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Code = c.Code
                }));

            // Act
            var result = await _categoryService.GetAllCategoriesAsync();

            // Assert
            Assert.Equal(expectedCategories, result, new CategoryResponseComparer());
        }

        public class CategoryResponseComparer : IEqualityComparer<CategoryResponse>
        {
            public bool Equals(CategoryResponse x, CategoryResponse y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;
                return x.Id == y.Id && x.Name == y.Name && x.Code == y.Code;
            }

            public int GetHashCode(CategoryResponse obj)
            {
                return HashCode.Combine(obj.Id, obj.Name, obj.Code);
            }
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldCreateCategory_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryRequest = new CategoryRequest
            {
                Name = "Clothing",
                Code = "CLTH",
                CreatedBy = Guid.NewGuid()
            };

            var newCategory = new Category
            {
                Id = Guid.NewGuid(),
                Name = categoryRequest.Name,
                Code = categoryRequest.Code.ToUpper(),
                CreatedBy = categoryRequest.CreatedBy,
                IsDeleted = false
            };

            var expectedResponse = new CategoryResponse
            {
                Id = newCategory.Id,
                Name = newCategory.Name,
                Code = newCategory.Code
            };

            _mockUnitOfWork.Setup(u => u.CategoryRepository.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
                .ReturnsAsync((Category)null);

            _mockUnitOfWork.Setup(u => u.CategoryRepository.AddAsync(It.IsAny<Category>()))
                .Callback<Category>(c => c = newCategory);

            _mockUnitOfWork.Setup(u => u.CommitAsync())
                .ReturnsAsync(1);

            _mockMapper.Setup(m => m.Map<CategoryResponse>(It.IsAny<Category>()))
                .Returns(expectedResponse);

            // Act
            var result = await _categoryService.CreateCategoryAsync(categoryRequest);

            // Assert
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldThrowArgumentException_WhenCategoryNameExists()
        {
            // Arrange
            var categoryRequest = new CategoryRequest
            {
                Name = "Clothing",
                Code = "CLTH",
                CreatedBy = Guid.NewGuid()
            };

            var existingCategory = new Category
            {
                Id = Guid.NewGuid(),
                Name = categoryRequest.Name,
                Code = "EXIST",
                CreatedBy = Guid.NewGuid(),
                IsDeleted = false
            };

            _mockUnitOfWork.Setup(u => u.CategoryRepository.GetAsync(It.Is<System.Linq.Expressions.Expression<Func<Category, bool>>>(e => e.Compile()(existingCategory))))
                .ReturnsAsync(existingCategory);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _categoryService.CreateCategoryAsync(categoryRequest));
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldThrowArgumentException_WhenCategoryCodeExists()
        {
            // Arrange
            var categoryRequest = new CategoryRequest
            {
                Name = "Clothing",
                Code = "CLTH",
                CreatedBy = Guid.NewGuid()
            };

            var existingCategory = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Other",
                Code = categoryRequest.Code.ToUpper(),
                CreatedBy = Guid.NewGuid(),
                IsDeleted = false
            };

            _mockUnitOfWork.Setup(u => u.CategoryRepository.GetAsync(It.Is<System.Linq.Expressions.Expression<Func<Category, bool>>>(e => e.Compile()(existingCategory) == false)))
                .ReturnsAsync((Category)null);

            _mockUnitOfWork.Setup(u => u.CategoryRepository.GetAsync(It.Is<System.Linq.Expressions.Expression<Func<Category, bool>>>(e => e.Compile()(existingCategory))))
                .ReturnsAsync(existingCategory);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _categoryService.CreateCategoryAsync(categoryRequest));
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldThrowException_WhenCommitFails()
        {
            // Arrange
            var categoryRequest = new CategoryRequest
            {
                Name = "Clothing",
                Code = "CLTH",
                CreatedBy = Guid.NewGuid()
            };

            _mockUnitOfWork.Setup(u => u.CategoryRepository.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
                .ReturnsAsync((Category)null);

            _mockUnitOfWork.Setup(u => u.CommitAsync())
                .ReturnsAsync(0);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _categoryService.CreateCategoryAsync(categoryRequest));
        }
    }
}
