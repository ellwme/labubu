using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<LabubuFigure> Figures => Set<LabubuFigure>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(u => u.Inventory)
            .WithOne(i => i.User)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InventoryItem>()
            .HasOne(i => i.Figure)
            .WithMany()
            .HasForeignKey(i => i.FigureId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LabubuFigure>().HasData(
            new LabubuFigure { Id = 1, Name = "Labubu Classic Pink", Rarity = "Common" },
            new LabubuFigure { Id = 2, Name = "Labubu Classic Blue", Rarity = "Common" },
            new LabubuFigure { Id = 3, Name = "Labubu Forest Green", Rarity = "Rare" },
            new LabubuFigure { Id = 4, Name = "Labubu Golden", Rarity = "Rare" },
            new LabubuFigure { Id = 5, Name = "Labubu Secret Rainbow", Rarity = "Legendary" }
        );
    }
}