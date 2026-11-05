using System.ComponentModel.DataAnnotations;

namespace Inventory.Models
{
    public class ServiceJob
    {
        public int Id { get; set; }

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

        public DateTime JobDate { get; set; } = DateTime.UtcNow;

        public decimal LabourCharges { get; set; }

        public decimal PartsTotal { get; set; }

        public decimal TotalAmount => PartsTotal + LabourCharges;

        [StringLength(500)]
        public string? Remarks { get; set; }

        public List<ServiceJobItem> Items { get; set; } = [];
    }

}
