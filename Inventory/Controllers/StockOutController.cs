using Inventory.Data;
using Inventory.Models;
using Inventory.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Controllers
{
    public class StockOutController(ApplicationDbContext db) : Controller
    {
        private async Task LoadProductsAsync()
        {
            //ViewBag.Products = await db.Products
            //    .AsNoTracking()
            //    .Where(p => p.IsActive)
            //    .OrderBy(p => p.Name)
            //    .Select(p => new SelectListItem
            //    {
            //        Value = p.Id.ToString(),
            //        Text = p.PartNumber + " - " + p.Name,
            //        Price = p.SellingPrice

            //    })
            //    .ToListAsync();

            ViewBag.Products = await db.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                //.Select(p => new
                //{
                //    Value = p.Id.ToString(),
                //    Text = p.PartNumber + " - " + p.Name,
                //    Price = p.PurchaseRate
                //})
                .ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Sale()
        {
            await LoadProductsAsync();

            return View(new CounterSaleViewModel
            {
                InvoiceNumber = "SALE-" + Guid.NewGuid().ToString("N")[..8]
                    .ToUpperInvariant()
            });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sale(CounterSaleViewModel model)
        {
            model.Items ??= [];

            if (model.Items.Count == 0)
                ModelState.AddModelError(
                    nameof(model.Items), "Add at least one item.");

            if (model.Items.Any(i => i.ProductId <= 0 || i.Quantity <= 0))
                ModelState.AddModelError(
                    nameof(model.Items), "Select an item and enter a positive quantity.");

            if (model.Items.GroupBy(i => i.ProductId).Any(g => g.Key <= 0 || g.Count() > 1))
                ModelState.AddModelError(
                    nameof(model.Items), "Select each product only once per sale.");

            if (await db.Sales.AnyAsync(
                s => s.InvoiceNumber == model.InvoiceNumber))
            {
                ModelState.AddModelError(
                    nameof(model.InvoiceNumber), "Invoice number already exists.");
            }

            if (!ModelState.IsValid)
            {
                await LoadProductsAsync();
                return View(model);
            }

            await using var tx = await db.Database.BeginTransactionAsync();

            var ids = model.Items.Select(i => i.ProductId).Distinct().ToList();

            var products = await db.Products
                .Where(p => ids.Contains(p.Id) && p.IsActive)
                .ToDictionaryAsync(p => p.Id);

            if (products.Count != ids.Count)
            {
                ModelState.AddModelError(
                    nameof(model.Items), "One or more selected products are unavailable.");
                await tx.RollbackAsync();
                await LoadProductsAsync();
                return View(model);
            }

            foreach (var line in model.Items)
            {
                var stock = await db.StockTransactions
                    .Where(t => t.ProductId == line.ProductId)
                    .SumAsync(t => (decimal?)t.QuantityChange) ?? 0m;

                if (stock < line.Quantity)
                {
                    ModelState.AddModelError(
                        nameof(model.Items),
                        $"Insufficient stock for {products[line.ProductId].Name}. Available: {stock}.");
                }
            }

            if (!ModelState.IsValid)
            {
                await tx.RollbackAsync();
                await LoadProductsAsync();
                return View(model);
            }

            var sale = new Sale
            {
                InvoiceNumber = model.InvoiceNumber.Trim(),
                CustomerName = model.CustomerName?.Trim(),
                CustomerPhone = model.CustomerPhone?.Trim(),
                SaleDate = DateTime.SpecifyKind(
                    model.SaleDate.Date, DateTimeKind.Utc)
            };

            foreach (var line in model.Items)
            {
                var product = products[line.ProductId];

                var item = new SaleItem
                {
                    ProductId = product.Id,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice
                };

                sale.Items.Add(item);
            }

            sale.TotalAmount = sale.Items.Sum(i => i.Quantity * i.UnitPrice);

            db.Sales.Add(sale);
            await db.SaveChangesAsync();

            foreach (var item in sale.Items)
            {
                db.StockTransactions.Add(new StockTransaction
                {
                    ProductId = item.ProductId,
                    MovementType = StockMovementType.CounterSale,
                    QuantityChange = -item.Quantity,
                    UnitCost = products[item.ProductId].PurchaseRate,
                    ReferenceNumber = sale.InvoiceNumber,
                    Remarks = "Counter sale",
                    TransactionDate = sale.SaleDate
                });
            }

            await db.SaveChangesAsync();
            await tx.CommitAsync();

            TempData["Success"] = $"Sale saved. Total: {sale.TotalAmount:N2}";
            return RedirectToAction(nameof(SalesHistory));
        }

        [HttpGet]
        public async Task<IActionResult> Service()
        {
            await LoadProductsAsync();

            return View(new ServiceJobViewModel
            {
                JobCardNumber = "JOB-" + Guid.NewGuid().ToString("N")[..8]
                    .ToUpperInvariant()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Service(ServiceJobViewModel model)
        {
            model.Items ??= [];

            if (model.Items.Count == 0)
                ModelState.AddModelError(
                    nameof(model.Items), "Add at least one item.");

            if (model.Items.Any(i => i.ProductId <= 0 || i.Quantity <= 0))
                ModelState.AddModelError(
                    nameof(model.Items), "Select an item and enter a positive quantity.");

            if (model.Items.GroupBy(i => i.ProductId).Any(g => g.Key <= 0 || g.Count() > 1))
                ModelState.AddModelError(
                    nameof(model.Items), "Select each product only once per job.");

            if (await db.ServiceJobs.AnyAsync(
                j => j.JobCardNumber == model.JobCardNumber))
            {
                ModelState.AddModelError(
                    nameof(model.JobCardNumber), "Job card number already exists.");
            }

            if (!ModelState.IsValid)
            {
                await LoadProductsAsync();
                return View(model);
            }

            await using var tx = await db.Database.BeginTransactionAsync();

            var ids = model.Items.Select(i => i.ProductId).Distinct().ToList();

            var products = await db.Products
                .Where(p => ids.Contains(p.Id) && p.IsActive)
                .ToDictionaryAsync(p => p.Id);

            if (products.Count != ids.Count)
            {
                ModelState.AddModelError(
                    nameof(model.Items), "One or more selected products are unavailable.");
                await tx.RollbackAsync();
                await LoadProductsAsync();
                return View(model);
            }

            foreach (var line in model.Items)
            {
                var stock = await db.StockTransactions
                    .Where(t => t.ProductId == line.ProductId)
                    .SumAsync(t => (decimal?)t.QuantityChange) ?? 0m;

                if (stock < line.Quantity)
                {
                    ModelState.AddModelError(
                        nameof(model.Items),
                        $"Insufficient stock for {products[line.ProductId].Name}. Available: {stock}.");
                }
            }

            if (!ModelState.IsValid)
            {
                await tx.RollbackAsync();
                await LoadProductsAsync();
                return View(model);
            }

            var job = new ServiceJob
            {
                JobCardNumber = model.JobCardNumber.Trim(),
                CustomerName = model.CustomerName?.Trim(),
                CustomerPhone = model.CustomerPhone?.Trim(),
                VehicleRegistration = model.VehicleRegistration?.Trim(),
                VehicleModel = model.VehicleModel?.Trim(),
                JobDate = DateTime.SpecifyKind(
                    model.JobDate.Date, DateTimeKind.Utc),
                LabourCharges = model.LabourCharges,
                Remarks = model.Remarks?.Trim()
            };

            foreach (var line in model.Items)
            {
                job.Items.Add(new ServiceJobItem
                {
                    ProductId = line.ProductId,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice
                });
            }

            job.PartsTotal = job.Items.Sum(i => i.Quantity * i.UnitPrice);

            db.ServiceJobs.Add(job);
            await db.SaveChangesAsync();

            foreach (var item in job.Items)
            {
                db.StockTransactions.Add(new StockTransaction
                {
                    ProductId = item.ProductId,
                    MovementType = StockMovementType.ServiceIssue,
                    QuantityChange = -item.Quantity,
                    UnitCost = products[item.ProductId].PurchaseRate,
                    ReferenceNumber = job.JobCardNumber,
                    Remarks = $"Service job: {job.VehicleRegistration}",
                    TransactionDate = job.JobDate
                });
            }

            await db.SaveChangesAsync();
            await tx.CommitAsync();

            TempData["Success"] =
                $"Service job saved. Customer bill: {job.TotalAmount:N2}";

            return RedirectToAction(nameof(ServiceHistory));
        }

        public async Task<IActionResult> SalesHistory()
        {
            var sales = await db.Sales
                .AsNoTracking()
                .OrderByDescending(s => s.SaleDate)
                .Take(500)
                .ToListAsync();

            return View(sales);
        }

        // Sale Details
        public async Task<IActionResult> Details(int id)
        {
            var sale = await db.Sales
                .Include(s => s.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null)
            {
                return NotFound();
            }

            return View(sale);
        }

        public async Task<IActionResult> ServiceHistory()
        {
            var jobs = await db.ServiceJobs
                .AsNoTracking()
                .OrderByDescending(j => j.JobDate)
                .Take(500)
                .ToListAsync();

            return View(jobs);
        }

        public async Task<IActionResult> ServiceDetails(int id)
        {
            var jobs = await db.ServiceJobs
                .Include(j => j.Items)
                    .ThenInclude(j => j.Product)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (jobs == null)
            {
                return NotFound();
            }

            //return null;
            return View(jobs);
        }

    }

}
