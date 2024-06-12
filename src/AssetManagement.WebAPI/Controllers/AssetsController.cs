using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AssetManagement.Domain.Entities;
using AssetManagement.Infrastructure.DataAccess;
using AssetManagement.Application.Services;
using AssetManagement.Application.Services.Implementations;
using AssetManagement.Application.Models.Responses;
using Org.BouncyCastle.Crypto;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManagement.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetsController : ControllerBase
    {
        private readonly IAssetService _assetService;

        public AssetsController(IAssetService assetService)
        {
            _assetService = assetService;
        }

    [HttpGet]
    public async Task<IActionResult> GetAllAssetAsync(int currentPage, string? code, string? name, string? categoryName, string? State, string? sortBy, string? sortOrder)   
    {
            try
            {
                Func<IQueryable<AssetResponse>, IOrderedQueryable<AssetResponse>>? orderBy;
                switch (sortBy?.ToLower())
                {
                    case "code":
                        orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.AssetCode) : x.OrderByDescending(a => a.AssetCode);
                        break;
                    case "name":
                        orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.AssetName) : x.OrderByDescending(a => a.AssetName);
                        break;
                    //case "categoryName":
                    //    orderBy = x => sortOrder != "desc" ? x.OrderBy(a => a.CategoryId => categoryName) : x.OrderByDescending(a => a.AssetName);
                    //    break;
                    default:
                        orderBy = null;
                        break;
                }
                return Ok(await _assetService.GetAllAssetAsync(
                    
                    ));
            }
            catch (Exception ex)
            {
                return  BadRequest(ex.Message);
            }
    }
    }
}
