using Inventory.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Controllers
{
    public class StockHistoryController(ApplicationDbContext db) : Controller
    {
        public async Task<IActionResult> Index(
            int? productId,
            DateTime? from,
            DateTime? to)
        {
            var query = db.StockTransactions
                .AsNoTracking()
                .Include(t => t.Product)
                .AsQueryable();

            if (productId.HasValue)
                query = query.Where(t => t.ProductId == productId.Value);

            if (from.HasValue)
            {
                var start = from.Value.Date;
                query = query.Where(t => t.TransactionDate >= start);
            }

            if (to.HasValue)
            {
                var endExclusive = to.Value.Date.AddDays(1);
                query = query.Where(t => t.TransactionDate < endExclusive);
            }

            ViewBag.Products = await db.Products
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewBag.ProductId = productId;
            ViewBag.From = from?.ToString("yyyy-MM-dd");
            ViewBag.To = to?.ToString("yyyy-MM-dd");

            var movements = await query
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.Id)
                .Take(500)
                .ToListAsync();

            return View(movements);
        }

    }
}