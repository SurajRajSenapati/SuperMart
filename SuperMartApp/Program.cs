using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SuperMartApp.Domain.Identity;
using SuperMartApp.Infrastructure.Data;
using SuperMartApp.Infrastructure.Seed;
using SuperMartApp.Web.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DbProvider: Sqlite (default) or SqlServer (set in appsettings)
var provider = builder.Configuration.GetValue<string>("DbProvider") ?? "PostgreSQL";
if (provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseNpgsql(builder.Configuration.GetConnectionString(provider)));
}
//else if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
//{
//    builder.Services.AddDbContext<AppDbContext>(opt =>
//        opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
//}
//else
//{
//    builder.Services.AddDbContext<AppDbContext>(opt =>
//        opt.UseSqlite(builder.Configuration.GetConnectionString("SqlServer")));
//}


var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"]!);

builder.Services
    .AddAuthentication() // don't change defaults; Identity cookie stays for MVC
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// DI for token service
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Identity + default UI (RCL)
builder.Services.AddDefaultIdentity<AppUser>(o =>
{
    o.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSwaggerGen();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SuperMartApp", Version = "v1" });
//    var securityScheme = new OpenApiSecurityScheme
//    {
//        Name = "Authorization",
//        Type = SecuritySchemeType.Http,
//        Scheme = "bearer",
//        BearerFormat = "JWT",
//        In = ParameterLocation.Header,
//        Description = "Enter 'Bearer {token}'",
//        Reference = new OpenApiReference
//        {
//            Type = ReferenceType.SecurityScheme,
//            Id = "Bearer"
//        }
//    };
//    c.AddSecurityDefinition("Bearer", securityScheme);
//    c.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        { securityScheme, new string[] { } }
//    });
//});

var app = builder.Build();

// Seed demo data
await DbSeeder.SeedAsync(app.Services);

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Identity UI

app.Run();
