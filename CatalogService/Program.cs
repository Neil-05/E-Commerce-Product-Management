using CatalogService.Data;
using CatalogService.Middleware;
using CatalogService.Repositories;
using CatalogService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)

    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate:
"{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [Catalog] [CID:{CorrelationId}] {Message:lj}{NewLine}{Exception}")


    .WriteTo.File("Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate:
        "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [Catalog] [CID:{CorrelationId}] {Message:lj}{NewLine}{Exception}")

    .CreateLogger();

  

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(3)
    )
);


builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
//builder.Services.AddScoped<IAuditRepository, AuditRepository>();
//builder.Services.AddScoped<IAuditService, AuditService>();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
                )
        };
    });

builder.Services.AddAuthorization();

// Correlation propagation for outgoing HTTP calls
builder.Services.AddTransient<CatalogService.Middleware.CorrelationPropagationHandler>();
builder.Services.AddHttpClient("default")
    .AddHttpMessageHandler<CatalogService.Middleware.CorrelationPropagationHandler>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.Configuration.GetValue<string>("DownstreamBaseUrl") ?? "http://localhost:5191"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<CatalogService.Middleware.CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.UseStaticFiles();

app.Run();