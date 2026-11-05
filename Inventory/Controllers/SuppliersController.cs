using Microsoft.AspNetCore.Mvc;

namespace Inventory.Controllers
{
    public class SuppliersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
