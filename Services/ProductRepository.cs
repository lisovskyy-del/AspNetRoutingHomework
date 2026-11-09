namespace AspNetRoutingHomework.Services;

using AspNetRoutingHomework.Models;

public class ProductRepository
{
    private readonly List<Product> _products = new();
    private int _nextId = 1;

    public ProductRepository()
    {
        
    }

    public List<Product> GetAll() => _products;

    public Product? GetById(int id) =>
        _products.FirstOrDefault(p => p.Id == id);

    public Product Create(string name, decimal price, string imageUrl)
    {
        var product = new Product
        {
            Id = _nextId++,
            Name = name,
            Price = price,
            ImageUrl = imageUrl
        };
        _products.Add(product);
        return product;
    }

    public Product? Update(int id, string name, decimal price)
    {
        var product = GetById(id);
        if (product == null) return null;

        product.Name = name;
        product.Price = price;

        return product;
    }

    public bool Delete(int id)
    {
        var product = GetById(id);
        if (product == null) return false;

        // delete image
        if (!string.IsNullOrEmpty(product.ImageUrl) && !product.ImageUrl.Contains("default.png"))
        {
            var relativePath = product.ImageUrl.TrimStart('/');
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        _products.Remove(product);
        return true;
    }
}