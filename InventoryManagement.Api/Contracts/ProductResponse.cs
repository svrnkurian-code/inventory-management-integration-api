namespace InventoryManagement.Api.Contracts;

public class ProductResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int QuantityInStock { get; set; }
}