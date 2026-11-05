using Microsoft.AspNetCore.Mvc;
using Inventory.Data;
using Inventory.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Controllers
{
    public class ReportsController(ApplicationDbContext db) : Controller
    {
        public async Task<IActionResult> Index(
            DateTime? from,
            DateTime? to,
            string type = "All",
            int? productId = null)
        {
            var query = db.StockTransactions
                .AsNoTracking()
                .Include(t => t.Product)
                .Where(t =>
                    t.MovementType == StockMovementType.CounterSale ||
                    t.MovementType == StockMovementType.ServiceIssue);

            if (from.HasValue)
            {
                var start = from.Value.Date;
                query = query.Where(t => t.TransactionDate >= start);
            }

            if (to.HasValue)
            {
                var end = to.Value.Date.AddDays(1);
                query = query.Where(t => t.TransactionDate < end);
            }

            if (type == "Sale")
            {
                query = query.Where(t =>
                    t.MovementType == StockMovementType.CounterSale);
            }
            else if (type == "Service")
            {
                query = query.Where(t =>
                    t.MovementType == StockMovementType.ServiceIssue);
            }

            if (productId.HasValue)
            {
                query = query.Where(t => t.ProductId == productId.Value);
            }

            var rows = await query
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.Id)
                .Take(5000)
                .Select(t => new StockOutReportRow
                {
                    Date = t.TransactionDate,
                    PartNumber = t.Product.PartNumber,
                    ProductName = t.Product.Name,
                    MovementType = t.MovementType,
                    ReferenceNumber = t.ReferenceNumber,
                    Quantity = -t.QuantityChange,
                    UnitCost = t.UnitCost,
                    UnitSellingPrice = t.MovementType ==
                        StockMovementType.CounterSale
                        ? db.SaleItems
                            .Where(i => i.Sale.InvoiceNumber == t.ReferenceNumber
                                     && i.ProductId == t.ProductId)
                            .Select(i => (decimal?)i.UnitPrice)
                            .FirstOrDefault() ?? 0m
                        : db.ServiceJobItems
                            .Where(i => i.ServiceJob.JobCardNumber == t.ReferenceNumber
                                     && i.ProductId == t.ProductId)
                            .Select(i => (decimal?)i.UnitPrice)
                            .FirstOrDefault() ?? 0m
                })
                .ToListAsync();

            ViewBag.From = from?.ToString("yyyy-MM-dd");
            ViewBag.To = to?.ToString("yyyy-MM-dd");
            ViewBag.Type = type;
            ViewBag.ProductId = productId;

            ViewBag.Products = await db.Products
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(rows);
        }

        [HttpGet]
        public async Task<IActionResult> ExportStockOutCsv(
            DateTime? from,
            DateTime? to,
            string type = "All",
            int? productId = null)
        {
            var query = db.StockTransactions
                .AsNoTracking()
                .Include(t => t.Product)
                .Where(t =>
                    t.MovementType == StockMovementType.CounterSale ||
                    t.MovementType == StockMovementType.ServiceIssue);

            if (from.HasValue)
                query = query.Where(t =>
                    t.TransactionDate >= from.Value.Date);

            if (to.HasValue)
            {
                var end = to.Value.Date.AddDays(1);
                query = query.Where(t => t.TransactionDate < end);
            }

            if (type == "Sale")
                query = query.Where(t =>
                    t.MovementType == StockMovementType.CounterSale);
            else if (type == "Service")
                query = query.Where(t =>
                    t.MovementType == StockMovementType.ServiceIssue);

            if (productId.HasValue)
                query = query.Where(t => t.ProductId == productId.Value);

            var rows = await query
                .OrderBy(t => t.TransactionDate)
                .Take(5000)
                .Select(t => new
                {
                    t.TransactionDate,
                    t.Product.PartNumber,
                    ProductName = t.Product.Name,
                    t.MovementType,
                    t.ReferenceNumber,
                    Quantity = -t.QuantityChange,
                    t.UnitCost
                })
                .ToListAsync();

            static string Csv(string? s) =>
                "\"" + (s ?? "").Replace("\"", "\"\"") + "\"";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(
                "Date,Part Code,Item,Type,Reference,Quantity,Unit Cost,Cost Value");

            foreach (var r in rows)
            {
                sb.AppendLine(string.Join(",",
                    Csv(r.TransactionDate.ToString("yyyy-MM-dd")),
                    Csv(r.PartNumber),
                    Csv(r.ProductName),
                    Csv(r.MovementType.ToString()),
                    Csv(r.ReferenceNumber),
                    r.Quantity.ToString(
                        System.Globalization.CultureInfo.InvariantCulture),
                    r.UnitCost.ToString(
                        System.Globalization.CultureInfo.InvariantCulture),
                    (r.Quantity * r.UnitCost).ToString(
                        System.Globalization.CultureInfo.InvariantCulture)));
            }

            var bytes = System.Text.Encoding.UTF8.GetPreamble()
                .Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString()))
                .ToArray();

            return File(bytes, "text/csv; charset=utf-8",
                $"StockOut-{DateTime.Today:yyyyMMdd}.csv");
        }
    }

    public class StockOutReportRow
    {
        public DateTime Date { get; set; }
        public string PartNumber { get; set; } = "";
        public string ProductName { get; set; } = "";
        public StockMovementType MovementType { get; set; }
        public string ReferenceNumber { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal UnitSellingPrice { get; set; }

        public decimal CostValue => Quantity * UnitCost;
        public decimal SalesValue => Quantity * UnitSellingPrice;
    }

}
