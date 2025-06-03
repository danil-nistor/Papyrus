using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Papyrus.Models;
using Papyrus.Data;
using Route = Papyrus.Models.Route;
using System.Reflection.Emit;

namespace Papyrus.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options){ }

    public DbSet<Document> Documents { get; set; }
    public DbSet<Status> Statuses { get; set; }
    public DbSet<Route> Routes { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Department> Departments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Route>()
            .HasKey(r => r.Id);
        /*
        // Отключаем каскадное удаление для всех связей
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }
        */
        // Seed начальных данных
        modelBuilder.Seed();
    }
}
