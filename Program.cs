
namespace ProductsCrudMinimalApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            var products = new List<Product>();

            app.MapGet("/products", () => Results.Ok(products)).RequireRateLimiting("PerClient");

            app.MapGet("/products/{id:int}", (int id) =>
            {
                var product = products.FirstOrDefault(p => p.Id == id);
                return product is not null ? Results.Ok(product) : Results.NotFound("Product not found");
            });

            app.MapPost("/products", (Product product) =>
            {
                if (string.IsNullOrWhiteSpace(product.Name) || product.Price < 0)
                    return Results.BadRequest("Invalid product data");

                product.Id = products.Any() ? products.Max(p => p.Id) + 1 : 1;
                products.Add(product);
                return Results.Created($"/products/{product.Id}", product);
            });

            app.MapPut("/products/{id:int}", (int id, Product updatedProduct) =>
            {
                var product = products.FirstOrDefault(p => p.Id == id);
                if (product is null) return Results.NotFound("Product not found");

                if (string.IsNullOrWhiteSpace(updatedProduct.Name) || updatedProduct.Price < 0)
                    return Results.BadRequest("Invalid product data");

                product.Name = updatedProduct.Name;
                product.Price = updatedProduct.Price;
                return Results.Ok(product);
            });

            app.MapDelete("/products/{id:int}", (int id) =>
            {
                var product = products.FirstOrDefault(p => p.Id == id);
                if (product is null) return Results.NotFound("Product not found");

                products.Remove(product);
                return Results.NoContent();
            });

            app.Run();
        }
    }
}
