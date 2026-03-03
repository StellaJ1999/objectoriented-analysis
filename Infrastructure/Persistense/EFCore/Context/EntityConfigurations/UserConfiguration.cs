using Domain.Users;
using Domain.Common.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistense.EFCore.Context.EntityConfigurations;


public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Primary Key
        builder.HasKey(u => u.Id);

        // Value Object: EmailAddress
        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value,
                value => EmailAddress.Create(value))
            .HasMaxLength(256)
            .IsRequired();

        // Unique index på email
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        // FullName
        builder.Property(u => u.FullName)
            .HasMaxLength(200)
            .IsRequired();

        // UserRole - spara som string
        builder.Property(u => u.Role)
            .HasConversion(
                role => role.Name,
                name => name == "Employee" ? UserRole.Employee : UserRole.Receptionist)
            .HasMaxLength(50)
            .IsRequired();

        // IsActive
        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Index för vanliga queries
        builder.HasIndex(u => u.IsActive)
            .HasDatabaseName("IX_Users_IsActive");

        builder.HasIndex(u => u.Role)
            .HasDatabaseName("IX_Users_Role");

        // Table name
        builder.ToTable("Users");
    }
}