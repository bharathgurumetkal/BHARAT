using Insurance.Application.Interfaces;
using Insurance.Application.Services;
using Insurance.Infrastructure.Data;
using Insurance.Infrastructure.Repositories;
using Insurance.Infrastructure.Services;
using Insurance.API.Middleware;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using System.Text;

namespace Insurance.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // -----------------------------
            // Add Controllers
            // -----------------------------
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // Serialize enums as their string names (e.g. "Active" not 2)
                    options.JsonSerializerOptions.Converters.Add(
                        new System.Text.Json.Serialization.JsonStringEnumConverter());
                });

            // -----------------------------
            // Database
            // -----------------------------
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            // -----------------------------
            // CORS (allow the Angular dev server)
            // -----------------------------
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", policy =>
                {
                    policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // -----------------------------
            // JWT Authentication
            // -----------------------------
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                    };
                });

            builder.Services.AddAuthorization();

            // -----------------------------
            // Dependency Injection
            // -----------------------------
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IPolicyService, PolicyService>();
            builder.Services.AddScoped<IPolicyRepository, PolicyRepository>();
            builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
            builder.Services.AddScoped<IAgentRepository, AgentRepository>();
            builder.Services.AddScoped<IClaimsOfficerRepository, ClaimsOfficerRepository>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<IClaimService, ClaimService>();
            builder.Services.AddScoped<IClaimRepository, ClaimRepository>();
            builder.Services.AddScoped<IPolicyProductRepository, PolicyProductRepository>();
            builder.Services.AddScoped<IPolicyProductService, PolicyProductService>();
            builder.Services.AddScoped<IPolicyApplicationRepository, PolicyApplicationRepository>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>(); // Register Repository
            builder.Services.AddScoped<ICommissionRepository, CommissionRepository>();
            builder.Services.AddHttpClient("AiService", client =>
            {
                client.BaseAddress = new Uri("http://localhost:5000/");
            });
            builder.Services.AddScoped<IAiClaimClient, AiClaimClient>();
            builder.Services.AddScoped<IPolicyApplicationService, PolicyApplicationService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();

            // -----------------------------
            // Swagger + JWT Support
            // -----------------------------
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
                    Description = "Enter 'Bearer' [space] and your token"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

            // -----------------------------
            // Middleware Pipeline
            // -----------------------------
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // apply CORS policy defined earlier MUST BE BEFORE STATIC FILES
            app.UseCors("AllowAngular");

            // Serve static files from "uploads" folder
            var uploadsDir = Path.Combine(builder.Environment.ContentRootPath, "uploads");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsDir),
                RequestPath = "/uploads"
            });

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseAuthentication();   // MUST be before Authorization
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}