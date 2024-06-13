using AssetManagement.Application.Services;
using AssetManagement.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace AssetManagement.WebAPI.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoriesController:ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoriesController(ICategoryService categoryService)
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
