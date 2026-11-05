using Inventory.Data;
using Inventory.Models;
using Inventory.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Controllers
{
    public class StockEntryController(ApplicationDbContext db) : Controller
    {
        private async Task LoadProductsAsync(
            ManualStockEntryViewModel model)
        {
            model.Products = await db.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.PartNumber + " - " + p.Name
                })
                .ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new ManualStockEntryViewModel();
            await LoadProductsAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ManualStockEntryViewModel model)
        {
            var product = await db.Products
                .SingleOrDefaultAsync(p =>
                    p.Id == model.ProductId && p.IsActive);

            if (product is null)
            {
                ModelState.AddModelError(
                    nameof(model.ProductId),
                    "Please select a valid active item.");
            }

            if (!ModelState.IsValid)
            {
                await LoadProductsAsync(model);
                return View(model);
            }

            var entry = new StockTransaction
            {
                ProductId = product!.Id,
                MovementType = StockMovementType.AdjustmentIn,
                QuantityChange = model.Quantity,
                UnitCost = model.UnitCost,
                ReferenceNumber =
                    $"MANUAL-IN-{Guid.NewGuid():N}",
                Remarks = string.IsNullOrWhiteSpace(model.Remarks)
                    ? "Manual stock entry"
                    : model.Remarks.Trim(),
                TransactionDate = DateTime.SpecifyKind(
                    model.EntryDate.Date,
                    DateTimeKind.Utc)
            };

            db.StockTransactions.Add(entry);
            await db.SaveChangesAsync();

            TempData["Success"] =
                $"Stock added successfully: {model.Quantity} {product!.Unit} of {product.Name}.";

            return RedirectToAction(nameof(Create));
        }
    }

}
