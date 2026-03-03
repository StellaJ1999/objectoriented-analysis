using Domain.Users;
using Domain.Bookings;
using Domain.Rooms;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistense.EFCore.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    { }

    // DbSets för aggregate roots
    public DbSet<User> Users => Set<User>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Room> Rooms => Set<Room>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Applicera alla entity configurations från assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Globala konventioner
        configurationBuilder.Properties<string>()
            .HaveMaxLength(500);

        base.ConfigureConventions(configurationBuilder);
    }
}