using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Infrastructure.Auth;
using Application.IServices;
using Infrastructure.DbContexts;
using Application.Common;
using Infrastructure.Time;
using Infrastructure.Services;


var builder = WebApplication.CreateBuilder(args);

// ===== Swagger =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LijanTeck API", Version = "v1" });

    // Bearer in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "????: Bearer {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
          Array.Empty<string>() }
    });
});
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var seed = builder.Configuration.GetSection("SeedAdmin");
    var email = seed["Email"];
    var pass = seed["Password"];
    var role = seed["RoleName"] ?? "Admin";
    var name = seed["DisplayName"] ?? "Admin";

    if (!string.IsNullOrWhiteSpace(email) && !await db.AdminUser.AnyAsync(u => u.Email == email))
    {
        db.AdminUser.Add(new Domain.Lijan.AdminUser
        {
            Email = email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(pass),
            RoleName = role,
            DisplayName = name,
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }
}

builder.Services.AddAuthorization(options =>
{
   
    options.AddPolicy("AdminsOnly", p => p.RequireRole("Admin", "Support", "Finance"));
    options.AddPolicy("CustomerOnly", p => p.RequireRole("Customer"));
});

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// ===== CORS ????????? =====
builder.Services.AddCors(o =>
{
    o.AddPolicy("Angular", p => p
        .WithOrigins("http://localhost:4200")   // ???? ???? ??????? ?? ???????
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// ===== DbContext (SQL Server) =====
// ????? ?? ???? ConnectionStrings:SystemsLocal ?? appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("dbcontext")));

// ===== AutoMapper =====
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// ===== JWT =====
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Missing Jwt:Key in configuration.");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ===== Register Auth Services =====
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IVatRateService, VatRateService>();
builder.Services.AddScoped<IPaymentMethodService, PaymentMethodService>();


builder.Services.AddHttpContextAccessor(); // ??? ?????? Claims ?? ??????

// Common services needed by ApplicationDbContext and services:
builder.Services.AddScoped<ICurrentUser,CurrentUserService>();
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

var app = builder.Build();

// ===== Pipeline =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("Angular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
