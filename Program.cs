using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using WebApplication1.Data;
using WebApplication1.Middleware;
using WebApplication1.Repositories;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// =========================
// 1. CONTROLLERS
// =========================
builder.Services.AddControllers();

// =========================
// 2. DATABASE
// =========================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// =========================
// 3. DEPENDENCY INJECTION
// =========================

// ----- User -----
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// ----- Farm -----
builder.Services.AddScoped<IFarmRepository, FarmRepository>();
builder.Services.AddScoped<IFarmService, FarmService>();

// ----- Field -----
builder.Services.AddScoped<IFieldRepository, FieldRepository>();
builder.Services.AddScoped<IFieldService, FieldService>();

// ----- Crop -----
builder.Services.AddScoped<ICropRepository, CropRepository>();
builder.Services.AddScoped<ICropService, CropService>();

// ----- Planting -----
builder.Services.AddScoped<IPlantingRepository, PlantingRepository>();
builder.Services.AddScoped<IPlantingService, PlantingService>();

// =========================
// 4. JWT AUTHENTICATION
// =========================
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]!
                )
            ),

            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });

// =========================
// 5. AUTHORIZATION
// =========================
builder.Services.AddAuthorization();

// =========================
// 6. SWAGGER + JWT
// =========================
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT Token"
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(
                "Bearer",
                document)] = []
        });
});

// =========================
// 7. BUILD
// =========================
var app = builder.Build();

// =========================
// 8. EXCEPTION MIDDLEWARE
// =========================
app.UseMiddleware<ExceptionMiddleware>();

// =========================
// 9. SWAGGER
// =========================
app.UseSwagger();
app.UseSwaggerUI();

// =========================
// 10. HTTPS
// =========================
app.UseHttpsRedirection();

// =========================
// 11. AUTHENTICATION
// =========================
app.UseAuthentication();

// =========================
// 12. AUTHORIZATION
// =========================
app.UseAuthorization();

// =========================
// 13. CONTROLLERS
// =========================
app.MapControllers();

// =========================
// 14. RUN
// =========================
app.Run();