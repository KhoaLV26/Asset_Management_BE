using AssetManagement.Application.Services;
using AssetManagement.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AssetManagement.WebAPI.Controllers
{
    [Route("api/roles")]
    [ApiController]
    public class RolesController : Controller
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleService.GetAllAsync();
            return Ok(new GeneralCreateResponse
            {
                Success = true,
                Message = "Roles retrieved successfully.",
                Data = roles
            });
        }
    }
}
