using System.ComponentModel.DataAnnotations;

namespace Inventory.ViewModels
{
    public class StockOutLineViewModel
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Range(typeof(decimal), "0.001", "999999999")]
        public decimal Quantity { get; set; }

        [Range(typeof(decimal), "0", "999999999")]
        public decimal UnitPrice { get; set; }
    }

    public class CounterSaleViewModel
    {
        [Required, StringLength(30)]
        public string InvoiceNumber { get; set; } = "";

        [StringLength(150)]
        public string? CustomerName { get; set; }

        [StringLength(20)]
        public string? CustomerPhone { get; set; }

        [DataType(DataType.Date)]
        public DateTime SaleDate { get; set; } = DateTime.Today;

        [MinLength(1, ErrorMessage = "Add at least one item.")]
        public List<StockOutLineViewModel> Items { get; set; } =
            [new StockOutLineViewModel()];
    }

    public class ServiceJobViewModel
    {
        [Required, StringLength(30)]
        public string JobCardNumber { get; set; } = "";

        [StringLength(150)]
        public string? CustomerName { get; set; }

        [StringLength(20)]
        public string? CustomerPhone { get; set; }

        [StringLength(30)]
        public string? VehicleRegistration { get; set; }

        [StringLength(100)]
        public string? VehicleModel { get; set; }

        [DataType(DataType.Date)]
        public DateTime JobDate { get; set; } = DateTime.Today;

        [Range(typeof(decimal), "0", "999999999")]
        public decimal LabourCharges { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        [MinLength(1, ErrorMessage = "Add at least one item.")]
        public List<StockOutLineViewModel> Items { get; set; } =
            [new StockOutLineViewModel()];
    }

}
