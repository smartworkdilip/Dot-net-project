using Microsoft.EntityFrameworkCore;
using ProductCrudApp.Api.Data;
using ProductCrudApp.Api.Repositories;
using ProductCrudApp.Api.Services;

// Application entry point: wires up dependency injection (DbContext, Repository, Service),
// builds the middleware pipeline, and starts the Kestrel web server.
var builder = WebApplication.CreateBuilder(args);

// Register MVC controllers and OpenAPI/Swagger metadata generation.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register EF Core's AppDbContext, configured to talk to SQL Server using the
// connection string from appsettings.json ("ConnectionStrings:DefaultConnection").
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register the Repository and Service layers for dependency injection.
// Scoped lifetime = one instance per HTTP request, matching AppDbContext's lifetime.
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

// In development, expose the OpenAPI document (used for API exploration/tooling).
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Map incoming requests to controller actions based on route attributes.
app.MapControllers();

app.Run();

// Exposed as partial so the test project's WebApplicationFactory<Program> can bootstrap
// this same app for integration testing.
public partial class Program { }
