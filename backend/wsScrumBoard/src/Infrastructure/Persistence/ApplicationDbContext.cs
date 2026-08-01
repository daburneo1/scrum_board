using Application.Ports.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<AppUser> Users => Set<AppUser>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<BoardColumn> BoardColumns => Set<BoardColumn>();

    public DbSet<BoardTask> BoardTasks => Set<BoardTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}