using Domain.Entities;
using Infrastructure.Persistence.Seed;
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
        
        builder.HasData(
            new
            {
                Id = SeedData.AdminUserId,
                Name = "Administrator",
                Email = "admin@scrumboard.local",
                NormalizedEmail = "ADMIN@SCRUMBOARD.LOCAL",
                PasswordHash = "AQAAAAIAAYagAAAAEOjwVSt9xPqj1MoHEqB6JWKKgecbqXFMJbIwl60PACRF1QcbrpDD+TZqUYW6erV45Q=="
            },
            new
            {
                Id = SeedData.ProjectUserId,
                Name = "Project User",
                Email = "user@scrumboard.local",
                NormalizedEmail = "USER@SCRUMBOARD.LOCAL",
                PasswordHash = "AQAAAAIAAYagAAAAEMS79DE2c4ZV0b3b9Ts8n5GN5mgPzcZ/dUj3jacSAZBO7p3ezMudh+CMtBiQuSOTqg=="
            });
    }
}