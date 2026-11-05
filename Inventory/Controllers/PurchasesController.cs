using Microsoft.AspNetCore.Mvc;

namespace Inventory.Controllers
{
    public class PurchasesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
