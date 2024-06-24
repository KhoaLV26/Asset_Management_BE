using AssetManagement.Application.Models.Requests;
using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Services;
using AssetManagement.Application.Services.Implementations;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Interfaces;
using AssetManagement.Domain.Models;
using AssetManagement.WebAPI.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AssetManagement.Test.Unit.CategoryControllerTest
{
    public class CategoryControllerTest
    {
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly CategoryController _controller;

        public CategoryControllerTest()
        {
            _mockCategoryService = new Mock<ICategoryService>();
            _controller = new CategoryController(_mockCategoryService.Object);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ReturnsOkResult_WithCategories()
        {
            // Arrange
            var categories = new List<CategoryResponse>
            {
                new CategoryResponse { Id = Guid.NewGuid(), Name = "Electronics", Code = "ELEC" },
                new CategoryResponse { Id = Guid.NewGuid(), Name = "Furniture", Code = "FURN" }
            };
            _mockCategoryService.Setup(service => service.GetAllCategoriesAsync()).ReturnsAsync(categories);

            // Act
            var result = await _controller.GetAllCategoriesAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralGetsResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Categories retrieved successfully.", response.Message);
            Assert.Equal(categories, response.Data);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ReturnsConflictResult_WhenNoCategories()
        {
            // Arrange
            _mockCategoryService.Setup(service => service.GetAllCategoriesAsync()).ReturnsAsync(new List<CategoryResponse>());

            // Act
            var result = await _controller.GetAllCategoriesAsync();

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal("No category.", response.Message);
        }

        [Fact]
        public async Task AddCategoryAsync_ReturnsOkResult_WithNewCategory()
        {
            // Arrange
            var categoryRequest = new CategoryRequest
            {
                Name = "Clothing",
                Code = "CLTH",
                CreatedBy = Guid.NewGuid()
            };
            var newCategory = new CategoryResponse
            {
                Id = Guid.NewGuid(),
                Name = categoryRequest.Name,
                Code = categoryRequest.Code.ToUpper()
            };
            _mockCategoryService.Setup(service => service.CreateCategoryAsync(categoryRequest)).ReturnsAsync(newCategory);

            // Act
            var result = await _controller.AddCategoryAsync(categoryRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GeneralGetResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Category added successfully.", response.Message);
            Assert.Equal(newCategory, response.Data);
        }

        [Fact]
        public async Task AddCategoryAsync_ReturnsConflictResult_WhenFailedToAddCategory()
        {
            // Arrange
            var categoryRequest = new CategoryRequest
            {
                Name = "Clothing",
                Code = "CLTH",
                CreatedBy = Guid.NewGuid()
            };
            _mockCategoryService.Setup(service => service.CreateCategoryAsync(categoryRequest)).ReturnsAsync((CategoryResponse)null);

            // Act
            var result = await _controller.AddCategoryAsync(categoryRequest);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Category added failed.", response.Message);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ReturnsConflictResult_WithException()
        {
            // Arrange
            var exception = new Exception("Something went wrong");
            _mockCategoryService.Setup(service => service.GetAllCategoriesAsync()).ThrowsAsync(exception);

            // Act
            var result = await _controller.GetAllCategoriesAsync();

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralGetsResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exception.Message, response.Message);
        }

        [Fact]
        public async Task AddCategoryAsync_ReturnsConflictResult_WithException()
        {
            // Arrange
            var categoryRequest = new CategoryRequest
            {
                Name = "Clothing",
                Code = "CLTH",
                CreatedBy = Guid.NewGuid()
            };
            var exception = new Exception("Failed to create category");
            _mockCategoryService.Setup(service => service.CreateCategoryAsync(categoryRequest)).ThrowsAsync(exception);

            // Act
            var result = await _controller.AddCategoryAsync(categoryRequest);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<GeneralBoolResponse>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Equal(exception.Message, response.Message);
        }
    }
}
