using System.ComponentModel.DataAnnotations;


namespace Inventory.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Part Code")]
        public string PartNumber { get; set; } = "";

        [Required, StringLength(150)]
        [Display(Name = "Item Name")]
        public string Name { get; set; } = "";

        [StringLength(60)]
        public string Category { get; set; } = "Spare Parts";

        [StringLength(20)]
        public string Unit { get; set; } = "Piece";

        [Range(typeof(decimal), "0", "999999999")]
        [Display(Name = "Purchase Rate")]
        public decimal PurchaseRate { get; set; }

        [Range(typeof(decimal), "0", "999999999")]
        [Display(Name = "Selling Price")]
        public decimal SellingPrice { get; set; }

        [Range(0, int.MaxValue)]
        [Display(Name = "Reorder Level")]
        public int ReorderLevel { get; set; } = 5;

        public bool IsActive { get; set; } = true;

    }
}
