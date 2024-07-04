using Microsoft.AspNetCore.Mvc;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AssetManagement.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {
        protected Guid UserID => Guid.Parse(FindClaim(ClaimTypes.Actor));
        protected Guid LocationID => Guid.Parse(FindClaim(ClaimTypes.Locality));
        protected string UserName => FindClaim(ClaimTypes.NameIdentifier).ToString();
        protected string CurrentToken => HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        private string FindClaim(string claimName)
        {
            var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var claim = claimsIdentity?.FindFirst(claimName);

            return claim?.Value;
        }
    }
}