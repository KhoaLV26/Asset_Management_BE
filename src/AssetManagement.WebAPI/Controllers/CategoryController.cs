using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.WebAPI.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
