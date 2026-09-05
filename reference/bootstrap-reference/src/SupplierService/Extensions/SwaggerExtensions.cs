namespace SupplierService.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSupplierSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        return services;
    }

    public static WebApplication UseSupplierSwagger(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()) return app;

        app.UseSwagger();
        app.UseSwaggerUI();
        return app;
    }
}
