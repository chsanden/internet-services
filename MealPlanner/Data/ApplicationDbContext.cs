using MealPlanner.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
    
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Dish> Dishes => Set<Dish>();
    public DbSet<Meal> Meals => Set<Meal>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<DishIngredient> DishIngredients => Set<DishIngredient>();
    public DbSet<MealDishes> MealDishes => Set<MealDishes>();
    public DbSet<PantryInventory> PantryInventory => Set<PantryInventory>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

       
        b.Entity<Unit>()
            .HasKey(u => u.UnitId);
        
        b.Entity<Unit>()
            .HasOne(u => u.BaseUnit)
            .WithMany()
            .HasForeignKey(u => u.BaseUnitId)
            .OnDelete(DeleteBehavior.Restrict);
        
        b.Entity<Ingredient>()
            .HasKey(x => x.IngredientId);
        
        b.Entity<Ingredient>()
            .HasOne(i => i.DefaultUnit)
            .WithMany()
            .HasForeignKey(i => i.DefaultUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Meal>()
            .HasKey(x => x.MealId);

        b.Entity<Dish>()
            .HasKey(x => x.DishId);

        b.Entity<Dish>(cfg =>
        {
            cfg.HasOne(d => d.Owner)
               .WithMany(u => u.Dishes)
               .HasForeignKey(d => d.OwnerId)
               .OnDelete(DeleteBehavior.Restrict);

            cfg.HasIndex(d => new { d.OwnerId, d.Name }).IsUnique();
        });
        
        b.Entity<MealDishes>()
            .HasKey(x => new { x.MealId, x.DishId });

        b.Entity<MealDishes>()
            .HasOne(x => x.Meal)
            .WithMany(x => x.Dishes)
            .HasForeignKey(x => x.MealId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<MealDishes>()
            .HasOne(x => x.Dish)
            .WithMany(x => x.Meals)
            .HasForeignKey(x => x.DishId)
            .OnDelete(DeleteBehavior.Cascade);
        
        b.Entity<DishIngredient>()
            .HasKey(x => new { x.DishId, x.IngredientId });

        b.Entity<DishIngredient>()
            .HasOne(x => x.Dish)
            .WithMany(x => x.DishIngredients)
            .HasForeignKey(x => x.DishId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<DishIngredient>()
            .HasOne(x => x.Ingredient)
            .WithMany(x => x.DishIngredients)
            .HasForeignKey(x => x.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<DishIngredient>()
            .HasOne(x => x.Unit)
            .WithMany()
            .HasForeignKey(x => x.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        // ---------- Pantry ----------
        b.Entity<PantryInventory>()
            .HasKey(x => new { x.UserId, x.IngredientId });

        b.Entity<PantryInventory>()
            .HasOne(x => x.User)
            .WithMany(u => u.Ingredients)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<PantryInventory>()
            .HasOne(x => x.Ingredient)
            .WithMany(i => i.Users)
            .HasForeignKey(x => x.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<PantryInventory>()
            .HasOne(x => x.Unit)
            .WithMany()
            .HasForeignKey(x => x.UnitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
