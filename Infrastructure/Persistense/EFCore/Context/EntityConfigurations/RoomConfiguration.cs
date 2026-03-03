using Domain.Rooms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistense.EFCore.Context.EntityConfigurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        // Primary Key
        builder.HasKey(r => r.Id);

        // Name
        builder.Property(r => r.Name)
            .HasMaxLength(100)
            .IsRequired();

        // Unique index på name
        builder.HasIndex(r => r.Name)
            .IsUnique()
            .HasDatabaseName("IX_Rooms_Name");

        // Capacity
        builder.Property(r => r.Capacity)
            .IsRequired();

        // Location
        builder.Property(r => r.Location)
            .HasMaxLength(200);

        // IsActive
        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Index för vanliga queries
        builder.HasIndex(r => r.IsActive)
            .HasDatabaseName("IX_Rooms_IsActive");

        builder.HasIndex(r => r.Capacity)
            .HasDatabaseName("IX_Rooms_Capacity");

        // Table name
        builder.ToTable("Rooms");
    }
}