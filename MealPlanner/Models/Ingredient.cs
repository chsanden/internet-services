using System.ComponentModel.DataAnnotations;

namespace MealPlanner.Models;

public class Ingredient
{
    public int IngredientId { get; set; }

    [Required, MaxLength(50)]
    public string IngredientName { get; set; } = string.Empty;
    
    public int DefaultUnitId { get; set; }
    public Unit DefaultUnit { get; set; } = null!;

    // navs
    public ICollection<DishIngredient> DishIngredients { get; set; } = new List<DishIngredient>();
    public ICollection<PantryInventory> Users { get; set; } = new List<PantryInventory>();
}