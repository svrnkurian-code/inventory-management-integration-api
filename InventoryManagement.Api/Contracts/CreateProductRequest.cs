using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Api.Contracts;

public class CreateProductRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int QuantityInStock { get; set; }
}