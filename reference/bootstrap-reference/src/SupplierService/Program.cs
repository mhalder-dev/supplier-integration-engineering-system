using SupplierService;
using SupplierService.Extensions;
using SupplierService.Middleware;

// Composition root only. Detail belongs in Extensions/ - see docs/architecture.md section 5.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSupplierOptions(builder.Configuration);
builder.Services.AddSupplierHttpClients();
builder.Services.AddSupplierValidators();
builder.Services.AddSupplierSwagger();
builder.Services.ConfigureServices();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSupplierSwagger();
app.MapEndpoints();

app.Run();

// Exposed so the test project can spin up the host if it ever needs to.
public partial class Program;
