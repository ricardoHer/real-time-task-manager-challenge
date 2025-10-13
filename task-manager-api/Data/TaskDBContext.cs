using Microsoft.EntityFrameworkCore;
using RealTimeTaskManager.API.Data.Models;

namespace RealTimeTaskManager.API.Data;

public class TaskDbContext(DbContextOptions<TaskDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
            entity.Property(x => x.TaskDescription).IsRequired().HasMaxLength(1000);
            entity.Property(x => x.CreateAt).IsRequired();
        });
    }
}