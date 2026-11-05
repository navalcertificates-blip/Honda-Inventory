using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inventory.ViewModels
{
    public class ManualStockEntryViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Select an item.")]
        public int ProductId { get; set; }

        [Range(typeof(decimal), "0.001", "999999999")]
        public decimal Quantity { get; set; }

        [Range(typeof(decimal), "0", "999999999")]
        [Display(Name = "Unit Cost")]
        public decimal UnitCost { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Entry Date")]
        public DateTime EntryDate { get; set; } = DateTime.Today;

        [StringLength(250)]
        public string? Remarks { get; set; }

        public List<SelectListItem> Products { get; set; } = [];

    }
}