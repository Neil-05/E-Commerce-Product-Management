using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AdminService.Services;

var builder = WebApplication.CreateBuilder(args);

// 🔥 Controllers
builder.Services.AddControllers();

// 🔥 HTTP Client (to call Catalog & Workflow)
builder.Services.AddHttpClient();

// 🔥 Services
builder.Services.AddScoped<IAdminService, AdminServiceManager>();

// 🔥 JWT Authentication
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

            // 🔥 OPTIONAL BUT GOOD
            NameClaimType = System.Security.Claims.ClaimTypes.Name,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
    });

// 🔥 Authorization
builder.Services.AddAuthorization();

// 🔥 Swagger (optional but recommended)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 🔥 Middleware
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();