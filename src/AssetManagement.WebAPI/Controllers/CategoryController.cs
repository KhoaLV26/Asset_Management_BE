using AssetManagement.Application.Models.Responses;
using AssetManagement.Application.Services;
using AssetManagement.Application.Services.Implementations;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using NuGet.ContentModel;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace AssetManagement.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController:ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                if (categories != null && categories.Any())
                {
                    return Ok(new GeneralGetsResponse
                    {
                        Success = true,
                        Message = "Categories retrieved successfully.",
                        Data = categories,
                    });
                }
                else
                {
                    return Conflict(new GeneralBoolResponse 
                    {
                        Success = false , 
                        Message = "No category."
                    });
                }
            }
            catch (Exception ex)
            {
                return Conflict(new GeneralGetsResponse
                {
                    Success = false,
                    Message = "Categories retrieved failed.",
                });
            }
        }
    }
}
