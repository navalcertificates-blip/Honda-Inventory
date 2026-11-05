using System.ComponentModel.DataAnnotations;

namespace Inventory.Models
{
    public enum StockMovementType
    {
        OpeningStock = 1,
        PurchaseReceipt = 2,
        CounterSale = 3,
        ServiceIssue = 4,
        CustomerReturn = 5,
        SupplierReturn = 6,
        AdjustmentIn = 7,
        AdjustmentOut = 8
    }

    public class StockTransaction
    {
        public long Id { get; set; }

        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        public StockMovementType MovementType { get; set; }

        // Positive quantity means stock received.
        // Negative quantity means stock issued.
        public decimal QuantityChange { get; set; }

        public decimal UnitCost { get; set; }

        [Required, StringLength(50)]
        public string ReferenceNumber { get; set; } = "";

        [StringLength(250)]
        public string? Remarks { get; set; }

        public DateTime TransactionDate { get; set; } =
            DateTime.UtcNow;

    }
}
