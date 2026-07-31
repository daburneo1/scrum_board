using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class AppUserConfiguration :
    IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .HasColumnName("id");

        builder.Property(user => user.Name)
            .HasColumnName("name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasColumnName("email")
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(user => user.NormalizedEmail)
            .HasColumnName("normalized_email")
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(user => user.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(512)
            .IsRequired();

        builder.HasIndex(user => user.NormalizedEmail)
            .IsUnique()
            .HasDatabaseName("ux_users_normalized_email");
    }
}