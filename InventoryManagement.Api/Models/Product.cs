using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Api.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0,int.MaxValue)]
        public int QuantityInStock { get; set; }

        public List<StockAdjustment> StockAdjustments { get; set; } = [];
    }
}
