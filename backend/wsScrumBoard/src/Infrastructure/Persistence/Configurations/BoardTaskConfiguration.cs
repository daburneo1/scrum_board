using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class BoardTaskConfiguration :
    IEntityTypeConfiguration<BoardTask>
{
    public void Configure(EntityTypeBuilder<BoardTask> builder)
    {
        builder.ToTable("board_tasks");

        builder.HasKey(task => task.Id);

        builder.Property(task => task.Id)
            .HasColumnName("id");

        builder.Property(task => task.Title)
            .HasColumnName("title")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(task => task.Description)
            .HasColumnName("description")
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(task => task.Priority)
            .HasColumnName("priority")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(task => task.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();

        builder.Property(task => task.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(task => task.ColumnId)
            .HasColumnName("column_id")
            .IsRequired();

        builder.Property(task => task.AssignedUserId)
            .HasColumnName("assigned_user_id");

        builder.HasOne(task => task.Column)
            .WithMany(column => column.Tasks)
            .HasForeignKey(task => task.ColumnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(task => task.AssignedUser)
            .WithMany(user => user.AssignedTasks)
            .HasForeignKey(task => task.AssignedUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(task => new
            {
                task.ColumnId,
                task.SortOrder
            })
            .HasDatabaseName("ix_board_tasks_column_order");

        builder.HasIndex(task => task.AssignedUserId)
            .HasDatabaseName("ix_board_tasks_assigned_user");

        builder.HasIndex(task => task.Priority)
            .HasDatabaseName("ix_board_tasks_priority");
    }
}