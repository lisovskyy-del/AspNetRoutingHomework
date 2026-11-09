using System.Text.Json;
using AspNetRoutingHomework.Services;
using AspNetRoutingHomework.Models;

namespace AspNetRoutingHomework.Middlewares;

public class RoutingAndProductMiddleware
{
    private readonly RequestDelegate _next;

    public RoutingAndProductMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context,
        ProductRepository repository)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        var method = context.Request.Method.ToUpper();

        
        // Html pages
        if (method == "GET" && (path == "/" || path == "/index.html"))
        {
            await ServeHtmlFile(context, "wwwroot/index.html");
            return;
        }

        if (method == "GET" && path == "/login")
        {
            await ServeHtmlFile(context, "wwwroot/login.html");
            return;
        }

        if (method == "POST" && path == "/login")
        {
            var form = await context.Request.ReadFormAsync();
            string username = form["username"].ToString();
            string password = form["password"].ToString();

            if (username == "admin" && password == "admin123")
            {
                context.Response.Cookies.Append("AuthUser", "admin", new CookieOptions 
                { 
                    HttpOnly = true,
                    Path = "/"
                });

                context.Response.Redirect("/products-page");
                return;
            }
    
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Invalid credentials!");
            return;
        }
        
        
        if (method == "GET" && path == "/products-page")
        {
            await ServeHtmlFile(context, "wwwroot/products.html");
            return;
        }

        // CRUD
        
        // read all
        if (method == "GET" && path == "/api/products")
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(repository.GetAll()));
            return;
        }
        
        // read by id

        if (method == "GET" && path.StartsWith("/api/products/"))
        {
            if (int.TryParse(path.Replace("/api/products/", ""), out int id))
            {
                var product = repository.GetById(id);

                if (product == null)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("Product not found");
                    return;
                }

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(product));
                return;
            }
        }
        
        // create

        if (method == "POST" && path == "/api/products")
        {
            if (!context.Request.HasFormContentType)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Form content expected");
                return;
            }

            var form = await context.Request.ReadFormAsync();
            string name = form["name"].ToString();
            string priceStr = form["price"].ToString();
            IFormFile? file = form.Files.GetFile("photo");
            
            // validation
            if (string.IsNullOrWhiteSpace(name) || name.Length < 2 || !decimal.TryParse(priceStr, out decimal price) || price <= 0)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Validation error: Invalid name or price.");
                return;
            }
            
            // save image
            string imagePath = "/images/default.png";
            if (file != null && file.Length > 0)
            {
                var extension = Path.GetExtension(file.FileName).ToLower();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                if (!allowed.Contains(extension))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Validation error: Invalid image extension.");
                    return;
                }

                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", uniqueFileName);

                Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                imagePath = $"/images/{uniqueFileName}";
            }
            
            var newProduct = repository.Create(name, price, imagePath);
            context.Response.StatusCode = StatusCodes.Status201Created;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(newProduct));
            return;
        }
        
        // Delete

        if (method == "DELETE" && path.StartsWith("/api/products/"))
        {
            if (int.TryParse(path.Replace("/api/products/", ""), out int id))
            {
                if (!repository.Delete(id))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("Product not found");
                    return;
                }
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsync("Deleted successfully");
                return;
            }
        }

        await _next(context);
    }
    
    private static async Task ServeHtmlFile(HttpContext context, string filePath)
    {
        if (!File.Exists(filePath))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsync("HTML template not found.");
            return;
        }

        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.SendFileAsync(filePath);
    }
}