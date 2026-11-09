using AspNetRoutingHomework.Middlewares;
using AspNetRoutingHomework.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ProductRepository>();

var app = builder.Build();

app.UseStaticFiles();

app.UseCustomAuth();
app.UseProductRouting();

app.Run();
