using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Admin.Data;
using Admin.Infrastructure;
using Admin.Infrastructure.Options;
using Admin.Infrastructure.Seeding;
using Admin.Services.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shared.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSharedServices(builder.Configuration);
builder.Services.AddAdminInfrastructure(builder.Configuration);

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwt = jwtSection.Get<JwtOptions>();
if (jwt is null || string.IsNullOrWhiteSpace(jwt.Secret) || jwt.Secret.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt configuration is missing or invalid. Set Jwt:Secret to at least 32 characters.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwt.Issuer,
        ValidateAudience = true,
        ValidAudience = jwt.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(2),
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var jti = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Jti);
            if (!string.IsNullOrWhiteSpace(jti))
            {
                var revocation = context.HttpContext.RequestServices
                    .GetRequiredService<IBusinessTokenRevocationService>();
                if (await revocation.IsRevokedAsync(jti, context.HttpContext.RequestAborted).ConfigureAwait(false))
                {
                    context.Fail("Session ended.");
                    return;
                }
            }

            if (context.Principal?.IsInRole("Platform") == true)
            {
                var staffIdValue = context.Principal.FindFirst("staff_id")?.Value;
                if (!Guid.TryParse(staffIdValue, out var staffId))
                {
                    context.Fail("Invalid platform session.");
                    return;
                }

                var dbPlatform = context.HttpContext.RequestServices.GetRequiredService<AdminDbContext>();
                var staffActive = await dbPlatform.PlatformStaff
                    .AsNoTracking()
                    .Where(s => s.Id == staffId)
                    .Select(s => s.IsActive)
                    .FirstOrDefaultAsync(context.HttpContext.RequestAborted)
                    .ConfigureAwait(false);

                if (!staffActive)
                {
                    context.Fail("Account blocked.");
                }

                return;
            }

            var userIdValue = context.Principal?.FindFirst("user_id")?.Value;
            if (!Guid.TryParse(userIdValue, out var userId))
            {
                return;
            }

            var db = context.HttpContext.RequestServices.GetRequiredService<AdminDbContext>();
            var isActive = await db.UserOrganizations
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.IsActive)
                .FirstOrDefaultAsync(context.HttpContext.RequestAborted)
                .ConfigureAwait(false);

            if (!isActive)
            {
                context.Fail("Account blocked.");
            }
        },
    };
});
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

var webRoot = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
Directory.CreateDirectory(Path.Combine(webRoot, "uploads"));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ArkifiHub API v1");
        options.RoutePrefix = "swagger";
    });
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => Results.Ok(new { name = "ArkifiHub API", version = "1.0" }));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
    if (app.Environment.IsProduction())
    {
        await db.Database.MigrateAsync().ConfigureAwait(false);
    }

    var menuLocationBackfill = scope.ServiceProvider.GetRequiredService<RestaurantMenuLocationBackfillService>();
    await menuLocationBackfill.BackfillAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<RestaurantMenuSeedService>();
    await seeder.SeedMissingMenusAsync(webRoot);

    var subscriptionSeeder = scope.ServiceProvider.GetRequiredService<BusinessSubscriptionSeedService>();
    await subscriptionSeeder.SeedMissingSubscriptionsAsync();

    var platformStaffSeeder = scope.ServiceProvider.GetRequiredService<PlatformStaffSeedService>();
    await platformStaffSeeder.SeedDefaultStaffAsync();
}

app.Run();
