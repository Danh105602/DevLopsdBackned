
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Security.Claims;
using System.Text;
using Serilog;

using WebApplication1.Authorization;
using WebApplication1.Data;
using WebApplication1.Middleware;
using WebApplication1.Repositories;
using WebApplication1.Services;

// ========================================
// 1. SERILOG INITIAL CONFIGURATION
// ========================================

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ========================================
    // 2. SERILOG CONFIGURATION
    // ========================================

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console();
    });

    // ========================================
    // 3. CONTROLLERS
    // ========================================

    builder.Services.AddControllers();

    // ========================================
    // 4. DATABASE CONFIGURATION
    // ========================================

    var connectionString =
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "DefaultConnection chưa được cấu hình trong appsettings.json!");

    // Cho phép lấy thông tin HTTP Request
    // phục vụ Audit Log
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));

    // ========================================
    // 5. DEPENDENCY INJECTION
    // ========================================

    // USER
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IUserService, UserService>();

    // FARM
    builder.Services.AddScoped<IFarmRepository, FarmRepository>();
    builder.Services.AddScoped<IFarmService, FarmService>();

    // FIELD
    builder.Services.AddScoped<IFieldRepository, FieldRepository>();
    builder.Services.AddScoped<IFieldService, FieldService>();

    // CROP
    builder.Services.AddScoped<ICropRepository, CropRepository>();
    builder.Services.AddScoped<ICropService, CropService>();

    // PLANTING
    builder.Services.AddScoped<IPlantingRepository, PlantingRepository>();
    builder.Services.AddScoped<IPlantingService, PlantingService>();

    // HARVEST
    builder.Services.AddScoped<IHarvestRepository, HarvestRepository>();
    builder.Services.AddScoped<IHarvestService, HarvestService>();

    // FILE
    builder.Services.AddScoped<IFileRepository, FileRepository>();
    builder.Services.AddScoped<IFileService, FileService>();

    // AUDIT LOG
    builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
    builder.Services.AddScoped<IAuditLogService, AuditLogService>();

    // OWNERSHIP
    builder.Services.AddScoped<IOwnershipService, OwnershipService>();

    // ========================================
    // 6. JWT CONFIGURATION
    // ========================================

    var jwtSettings = builder.Configuration.GetSection("Jwt");

    var secretKey = jwtSettings["Key"]
        ?? throw new InvalidOperationException(
            "Jwt:Key chưa được cấu hình!");

    var issuer = jwtSettings["Issuer"];
    var audience = jwtSettings["Audience"];

    if (string.IsNullOrWhiteSpace(issuer) ||
        string.IsNullOrWhiteSpace(audience))
    {
        throw new InvalidOperationException(
            "Jwt:Issuer hoặc Jwt:Audience chưa được cấu hình!");
    }

    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme =
                JwtBearerDefaults.AuthenticationScheme;

            options.DefaultChallengeScheme =
                JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secretKey)
                    ),

                    ValidateIssuer = true,
                    ValidIssuer = issuer,

                    ValidateAudience = true,
                    ValidAudience = audience,

                    ValidateLifetime = true,

                    RoleClaimType = ClaimTypes.Role,

                    ClockSkew = TimeSpan.Zero
                };
        });

    // ========================================
    // 7. AUTHORIZATION + PERMISSION POLICIES
    // ========================================

    builder.Services.AddAuthorization(options =>
    {
        // ADMIN ONLY
        options.AddPolicy("AdminOnly", policy =>
            policy.RequireRole("Admin"));

        // FARMER OR ADMIN
        options.AddPolicy("FarmerOrAdmin", policy =>
            policy.RequireRole("Farmer", "Admin"));

        // PERMISSION POLICIES
        foreach (var permission in Permissions.All)
        {
            options.AddPolicy(permission, policy =>
                policy.RequireClaim("permission", permission));
        }
    });

    // ========================================
    // 8. SWAGGER CONFIGURATION + JWT
    // ========================================

    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Smart Agriculture API",
            Version = "v1",
            Description = "API quản lý nông nghiệp thông minh"
        });

        // JWT BEARER AUTHENTICATION
        options.AddSecurityDefinition("Bearer",
            new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Nhập JWT Token để xác thực API"
            });

        // SWAGGER AUTHORIZE
        options.AddSecurityRequirement(document =>
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(
                    "Bearer",
                    document)] = []
            });
    });

    // ========================================
    // 9. BUILD APPLICATION
    // ========================================

    var app = builder.Build();

    // ========================================
    // 10. GLOBAL EXCEPTION HANDLING
    // ========================================

    app.UseMiddleware<ExceptionMiddleware>();

    // ========================================
    // 11. SERILOG REQUEST LOGGING
    // ========================================

    app.UseSerilogRequestLogging();

    // ========================================
    // 12. SWAGGER
    // ========================================

    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Smart Agriculture API V1"
        );

        options.DocumentTitle = "Smart Agriculture API";
    });

    // ========================================
    // 13. HTTPS REDIRECTION
    // ========================================

    app.UseHttpsRedirection();

    // ========================================
    // 14. AUTHENTICATION
    // ========================================

    app.UseAuthentication();

    // ========================================
    // 15. AUTHORIZATION
    // ========================================

    app.UseAuthorization();

    // ========================================
    // 16. MAP CONTROLLERS
    // ========================================

    app.MapControllers();

    // ========================================
    // 17. APPLICATION START
    // ========================================

    Log.Information("========================================");
    Log.Information("Smart Agriculture API is starting...");
    Log.Information("Environment: {Environment}",
        app.Environment.EnvironmentName);
    Log.Information("========================================");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Smart Agriculture API stopped unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
