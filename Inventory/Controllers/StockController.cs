using Inventory.Data;
using Inventory.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Controllers
{
    public class StockController(ApplicationDbContext db) : Controller
    {
        public async Task<IActionResult> Index(string? search)
        {
            var query = db.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    Product = p,
                    Quantity = db.StockTransactions
                        .Where(t => t.ProductId == p.Id)
                        .Sum(t => (decimal?)t.QuantityChange) ?? 0m
                });

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.Product.PartNumber.Contains(search) ||
                    x.Product.Name.Contains(search));
            }

            var result = await query
                .OrderBy(x => x.Product.Name)
                .Select(x => new Inventory.ViewModels.StockRow
                {
                    ProductId = x.Product.Id,
                    PartNumber = x.Product.PartNumber,
                    Name = x.Product.Name,
                    Category = x.Product.Category,
                    Unit = x.Product.Unit,
                    Quantity = x.Quantity,
                    ReorderLevel = x.Product.ReorderLevel,
                    StockValue = x.Quantity * x.Product.PurchaseRate
                })
                .ToListAsync();

            ViewBag.Search = search;
            return View(result);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new ManualStockEntryViewModel();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ExportCsv(string? search)
        {
            var query = db.Products
                .AsNoTracking()
                .Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.PartNumber.Contains(search) ||
                    p.Name.Contains(search));
            }

            var products = await query
                .OrderBy(p => p.Name)
                .Select(p => new
                {
                    p.PartNumber,
                    p.Name,
                    p.Category,
                    p.Unit,
                    p.PurchaseRate,
                    p.ReorderLevel,
                    Quantity = db.StockTransactions
                        .Where(t => t.ProductId == p.Id)
                        .Sum(t => (decimal?)t.QuantityChange) ?? 0m
                })
                .ToListAsync();

            static string Csv(string? value) =>
                "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(
                "Part Code,Item,Category,Unit,Available Quantity,Purchase Rate,Stock Value,Reorder Level,Status");

            foreach (var p in products)
            {
                var value = p.Quantity * p.PurchaseRate;
                var status = p.Quantity <= p.ReorderLevel ? "Reorder" : "OK";

                sb.AppendLine(string.Join(",",
                    Csv(p.PartNumber),
                    Csv(p.Name),
                    Csv(p.Category),
                    Csv(p.Unit),
                    p.Quantity.ToString(
                        System.Globalization.CultureInfo.InvariantCulture),
                    p.PurchaseRate.ToString(
                        System.Globalization.CultureInfo.InvariantCulture),
                    value.ToString(
                        System.Globalization.CultureInfo.InvariantCulture),
                    p.ReorderLevel,
                    Csv(status)));
            }

            return File(
                System.Text.Encoding.UTF8.GetPreamble()
                    .Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString()))
                    .ToArray(),
                "text/csv; charset=utf-8",
                $"CurrentStock-{DateTime.Today:yyyyMMdd}.csv");
        }

    }

}
