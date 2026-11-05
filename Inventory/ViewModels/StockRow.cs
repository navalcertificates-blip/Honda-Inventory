namespace Inventory.ViewModels
{
    public class StockRow
    {
        public int ProductId { get; set; }
        public string PartNumber { get; set; } = "";
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal Quantity { get; set; }
        public int ReorderLevel { get; set; }
        public decimal StockValue { get; set; }

        public bool IsLowStock => Quantity <= ReorderLevel;

    }
}
