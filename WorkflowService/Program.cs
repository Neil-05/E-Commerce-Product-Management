using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WorkflowService.Data;
using WorkflowService.Services;

using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)

    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate:
"{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [Workflow] [CID:{CorrelationId}] {Message:lj}{NewLine}{Exception}")


    .WriteTo.File("Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate:
        "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [Workflow] [CID:{CorrelationId}] {Message:lj}{NewLine}{Exception}")

    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Controllers
builder.Services.AddControllers();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        sql => sql.EnableRetryOnFailure()
    ));

// Services
builder.Services.AddScoped<IWorkflowService, WorkflowManager>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<WorkflowService.Middleware.CorrelationPropagationHandler>();
builder.Services.AddHttpClient("catalog")
    .AddHttpMessageHandler<WorkflowService.Middleware.CorrelationPropagationHandler>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.Configuration.GetValue<string>("CatalogBaseUrl") ?? "http://localhost:5191"));
builder.Services.AddScoped<WorkflowService.Services.ISagaManager, WorkflowService.Services.SagaManager>();

// JWT Authentication
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
                ),
            NameClaimType = System.Security.Claims.ClaimTypes.Name,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
    });

// Authorization
builder.Services.AddAuthorization();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Workflow API",
        Version = "v1"
    });

    // JWT support in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Swagger Middleware
app.UseSwagger();
app.UseSwaggerUI();

// Auth Middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<WorkflowService.Middleware.CorrelationIdMiddleware>();
app.UseMiddleware<WorkflowService.Middleware.RequestLoggingMiddleware>();

// Map Controllers
app.MapControllers();

app.Run();