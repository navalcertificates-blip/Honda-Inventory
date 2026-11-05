using System.ComponentModel.DataAnnotations;

namespace Inventory.Models
{
    public class ServiceJobItem
    {
        public int Id { get; set; }

        public int ServiceJobId { get; set; }
        public ServiceJob ServiceJob { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [Range(typeof(decimal), "0.001", "999999999")]
        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal LineTotal => Quantity * UnitPrice;
    }

}
