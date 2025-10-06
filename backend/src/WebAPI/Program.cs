using ProductCatalogAPI.Application.Exceptions;
using ProductCatalogAPI.Application.UseCases.Orders;
using ProductCatalogAPI.Application.UseCases.Products;
using ProductCatalogAPI.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

//Orders
builder.Services.AddScoped<CreateOrderUseCase>();
builder.Services.AddScoped<GetAllOrdersUserCase>();
builder.Services.AddScoped<GetOrderByIdUseCase>();

builder.Services.AddScoped<CreateProductUseCase>();
builder.Services.AddScoped<DecreaseStockAdminUseCase>();
builder.Services.AddScoped<DeleteProductUseCase>();
builder.Services.AddScoped<GetAllProductsUseCase>();
builder.Services.AddScoped<GetProductByIdUseCase>();
builder.Services.AddScoped<IncreaseStockUseCase>();
builder.Services.AddScoped<UpdateProductUseCase>();


// Get CORS settings from configuration
var corsSettings = builder.Configuration.GetSection("Cors");
var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>();

// If no origins configured, use sensible defaults based on environment
if (allowedOrigins == null || allowedOrigins.Length == 0)
{
    allowedOrigins = builder.Environment.IsDevelopment() 
        ? new[] { "http://localhost:3000", "http://frontend:3000", "https://localhost:3000" }
        : new[] { "https://domain.com", "https://www.domain.com" };
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Use(async (ctx, next) =>
{
    var header = "X-Correlation-ID";
    var correlationId = ctx.Request.Headers.TryGetValue(header, out var v) && !string.IsNullOrWhiteSpace(v)
        ? v.ToString()
        : Guid.NewGuid().ToString("n");
    ctx.Items[header] = correlationId;
    ctx.Response.Headers[header] = correlationId;

    using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});

app.UseSerilogRequestLogging(opts =>
{
    opts.GetLevel = (httpContext, elapsed, ex) =>
        ex != null || httpContext.Response.StatusCode >= 500
            ? Serilog.Events.LogEventLevel.Error
            : elapsed > 1000 ? Serilog.Events.LogEventLevel.Warning
                : Serilog.Events.LogEventLevel.Information;
});

app.Use(async (ctx, next) =>
{
    try
    {
        await next();
    }
    catch (InsufficientStockException ex)
    {
        Log.Warning(ex, "InsufficientStock: {Path}", ctx.Request.Path);
        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
        await ctx.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
    catch (ConcurrencyException ex)
    {
        Log.Warning(ex, "ConcurrencyConflict: {Path}", ctx.Request.Path);
        ctx.Response.StatusCode = StatusCodes.Status409Conflict;
        await ctx.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
    catch (NotFoundException ex)
    {
        Log.Information("NotFound: {Message} {Path}", ex.Message, ctx.Request.Path);
        ctx.Response.StatusCode = StatusCodes.Status404NotFound;
        await ctx.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Unhandled error at {Path}", ctx.Request.Path);
        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await ctx.Response.WriteAsJsonAsync(new { error = "Error interno" });
    }
});

app.UseCors("CorsPolicy");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


app.Run();

public partial class Program { }