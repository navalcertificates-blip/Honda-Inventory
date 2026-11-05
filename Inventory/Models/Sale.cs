using System.ComponentModel.DataAnnotations;

namespace Inventory.Models
{
    public class Sale
    {
        public int Id { get; set; }

        [Required, StringLength(30)]
        public string InvoiceNumber { get; set; } = "";

        [StringLength(150)]
        public string? CustomerName { get; set; }

        [StringLength(20)]
        public string? CustomerPhone { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.UtcNow;

        public decimal TotalAmount { get; set; }

        public List<SaleItem> Items { get; set; } = [];
    }

}
