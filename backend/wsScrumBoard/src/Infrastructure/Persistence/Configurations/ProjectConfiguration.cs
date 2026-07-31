using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class ProjectConfiguration :
    IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");

        builder.HasKey(project => project.Id);

        builder.Property(project => project.Id)
            .HasColumnName("id");

        builder.Property(project => project.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(project => project.Description)
            .HasColumnName("description")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(project => project.StartDate)
            .HasColumnName("start_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(project => project.ExpectedEndDate)
            .HasColumnName("expected_end_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(project => project.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(project => project.Name)
            .HasDatabaseName("ix_projects_name");
    }
}