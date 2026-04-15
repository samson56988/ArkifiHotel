using Admin.Data.Configurations;
using Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Admin.Data;

public class AdminDbContext : DbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options)
        : base(options)
    {
    }

    public DbSet<BusinessRegistration> BusinessRegistrations => Set<BusinessRegistration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new BusinessRegistrationConfiguration());
    }
}
