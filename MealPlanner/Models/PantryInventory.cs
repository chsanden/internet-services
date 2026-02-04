using MealPlanner.Data;

namespace MealPlanner.Models;

public class PantryInventory
{
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    public int IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;

    public decimal? Amount { get; set; }
    
    public int? UnitId { get; set; }
    public Unit? Unit { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}