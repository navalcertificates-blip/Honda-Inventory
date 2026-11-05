using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Inventory.Data;
using Inventory.Models;


namespace Inventory.Controllers
{
    public class ProductsController(ApplicationDbContext db) : Controller
    {
        public async Task<IActionResult> Index(string? search)
        {
            var query = db.Products.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.PartNumber.Contains(search) ||
                    p.Name.Contains(search));
            }

            ViewBag.Search = search;

            return View(await query
                .OrderBy(p => p.Name)
                .ToListAsync());
        }

        public IActionResult Create()
        {
            return View(new Product());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (await db.Products.AnyAsync(
                p => p.PartNumber == product.PartNumber))
            {
                ModelState.AddModelError(
                    nameof(product.PartNumber),
                    "This part code already exists.");
            }

            if (!ModelState.IsValid)
                return View(product);

            db.Products.Add(product);
            await db.SaveChangesAsync();

            TempData["Success"] = "Product created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await db.Products.FindAsync(id);

            if (product is null)
                return NotFound();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id)
                return BadRequest();

            if (await db.Products.AnyAsync(
                p => p.PartNumber == product.PartNumber && p.Id != id))
            {
                ModelState.AddModelError(
                    nameof(product.PartNumber),
                    "This part code already exists.");
            }

            if (!ModelState.IsValid)
                return View(product);

            var existing = await db.Products.FindAsync(id);

            if (existing is null)
                return NotFound();

            existing.PartNumber = product.PartNumber;
            existing.Name = product.Name;
            existing.Category = product.Category;
            existing.Unit = product.Unit;
            existing.PurchaseRate = product.PurchaseRate;
            existing.SellingPrice = product.SellingPrice;
            existing.ReorderLevel = product.ReorderLevel;
            existing.IsActive = product.IsActive;

            await db.SaveChangesAsync();

            TempData["Success"] = "Product updated successfully.";
            return RedirectToAction(nameof(Index));
        }

    }
}