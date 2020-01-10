using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers
{
    public class ApiHelpController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}