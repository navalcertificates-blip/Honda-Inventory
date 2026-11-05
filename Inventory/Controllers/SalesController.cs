using Microsoft.AspNetCore.Mvc;

namespace Inventory.Controllers
{
    public class SalesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
