using Domain.Bookings;
using Domain.Common.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistense.EFCore.Context.EntityConfigurations;

/// <summary>
/// EF Core configuration för Booking-aggregat
/// </summary>
public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        // Primary Key
        builder.HasKey(b => b.Id);

        // Value Object: TimeInterval (Owned Entity)
        builder.OwnsOne(b => b.TimeInterval, timeInterval =>
        {
            timeInterval.Property(t => t.StartTime)
                .HasColumnName("StartTime")
                .IsRequired();

            timeInterval.Property(t => t.EndTime)
                .HasColumnName("EndTime")
                .IsRequired();

            // Index för att snabbt hitta överlappande bokningar
            timeInterval.WithOwner();
        });

        // RoomId - Foreign Key
        builder.Property(b => b.RoomId)
            .IsRequired();

        // UserId - Foreign Key
        builder.Property(b => b.UserId)
            .IsRequired();

        // Purpose
        builder.Property(b => b.Purpose)
            .HasMaxLength(500);

        // Status
        builder.Property(b => b.Status)
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue("Active");

        // Indexes för vanliga queries
        builder.HasIndex(b => b.RoomId)
            .HasDatabaseName("IX_Bookings_RoomId");

        builder.HasIndex(b => b.UserId)
            .HasDatabaseName("IX_Bookings_UserId");

        builder.HasIndex(b => b.Status)
            .HasDatabaseName("IX_Bookings_Status");

        // Composite index för att hitta aktiva bokningar per rum
        builder.HasIndex(b => new { b.RoomId, b.Status })
            .HasDatabaseName("IX_Bookings_RoomId_Status");

        // Table name
        builder.ToTable("Bookings");
    }
}