using System.ComponentModel.DataAnnotations;

namespace Inventory.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = "";

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        public bool IsActive { get; set; } = true;

    }
}
