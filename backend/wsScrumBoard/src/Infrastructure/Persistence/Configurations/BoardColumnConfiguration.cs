using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class BoardColumnConfiguration :
    IEntityTypeConfiguration<BoardColumn>
{
    public void Configure(EntityTypeBuilder<BoardColumn> builder)
    {
        builder.ToTable("board_columns");

        builder.HasKey(column => column.Id);

        builder.Property(column => column.Id)
            .HasColumnName("id");

        builder.Property(column => column.Name)
            .HasColumnName("name")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(column => column.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();

        builder.Property(column => column.ProjectId)
            .HasColumnName("project_id")
            .IsRequired();

        builder.HasOne(column => column.Project)
            .WithMany(project => project.Columns)
            .HasForeignKey(column => column.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(column => new
            {
                column.ProjectId,
                column.SortOrder
            })
            .HasDatabaseName("ix_board_columns_project_order");
    }
}