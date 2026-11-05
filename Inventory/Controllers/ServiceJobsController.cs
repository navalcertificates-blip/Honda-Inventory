using Microsoft.AspNetCore.Mvc;

namespace Inventory.Controllers
{
    public class ServiceJobsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
