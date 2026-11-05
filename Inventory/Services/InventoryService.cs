using Microsoft.EntityFrameworkCore;
using Inventory.Data;
using Inventory.Models;

namespace Inventory.Services;

public class InventoryService(ApplicationDbContext db)
{
    public async Task<decimal> GetAvailableStockAsync(int productId)
    {
        return await db.StockTransactions
            .Where(t => t.ProductId == productId)
            .SumAsync(t => t.QuantityChange);
    }

    public async Task<decimal> GetTotalStockAsync()
    {
        var totals = await db.StockTransactions
            .GroupBy(t => t.ProductId)
            .Select(g => g.Sum(t => t.QuantityChange))
            .ToListAsync();

        return totals.Sum();
    }

    public async Task RecordOpeningStockAsync(
        int productId,
        decimal quantity,
        decimal unitCost)
    {
        if (quantity < 0)
            throw new ArgumentException(
                "Opening quantity cannot be negative.");

        if (unitCost < 0)
            throw new ArgumentException(
                "Unit cost cannot be negative.");

        await using var tx = await db.Database.BeginTransactionAsync();

        var product = await db.Products
            .SingleOrDefaultAsync(p => p.Id == productId && p.IsActive);

        if (product is null)
            throw new InvalidOperationException(
                "Product not found or inactive.");

        var existing = await db.StockTransactions.AnyAsync(t =>
            t.ProductId == productId &&
            t.MovementType == StockMovementType.OpeningStock);

        if (existing)
            throw new InvalidOperationException(
                "Opening stock has already been recorded for this item.");

        if (quantity > 0)
        {
            db.StockTransactions.Add(new StockTransaction
            {
                ProductId = productId,
                MovementType = StockMovementType.OpeningStock,
                QuantityChange = quantity,
                UnitCost = unitCost,
                ReferenceNumber = $"OPEN-{productId}",
                Remarks = "Initial opening balance"
            });

            await db.SaveChangesAsync();
        }

        await tx.CommitAsync();
    }

    public async Task IssueStockAsync(
        int productId,
        decimal quantity,
        StockMovementType movementType,
        string referenceNumber,
        string? remarks = null)
    {
        if (quantity <= 0)
            throw new ArgumentException(
                "Issue quantity must be greater than zero.");

        if (string.IsNullOrWhiteSpace(referenceNumber))
            throw new ArgumentException(
                "A reference number is required.");

        if (movementType is not StockMovementType.CounterSale
            and not StockMovementType.ServiceIssue
            and not StockMovementType.SupplierReturn
            and not StockMovementType.AdjustmentOut)
        {
            throw new ArgumentException("Invalid issue type.");
        }

        await using var tx = await db.Database.BeginTransactionAsync();

        var product = await db.Products
            .SingleOrDefaultAsync(p => p.Id == productId && p.IsActive);

        if (product is null)
            throw new InvalidOperationException(
                "Product not found or inactive.");

        var currentStock = await GetAvailableStockAsync(productId);

        if (currentStock < quantity)
            throw new InvalidOperationException(
                $"Insufficient stock. Available quantity: {currentStock}");

        db.StockTransactions.Add(new StockTransaction
        {
            ProductId = productId,
            MovementType = movementType,
            QuantityChange = -quantity,
            UnitCost = product.PurchaseRate,
            ReferenceNumber = referenceNumber.Trim(),
            Remarks = remarks
        });

        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }
}
