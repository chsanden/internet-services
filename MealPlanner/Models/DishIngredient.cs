using System.ComponentModel.DataAnnotations;

namespace MealPlanner.Models;

// join table for Recipes and Ingredients
public class DishIngredient
{
    public int DishId { get; set; }
    public Dish Dish { get; set; } = null!;

    public int IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;
    
    [Range(0, 100000)]
    public decimal Amount { get; set; }
    
    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;

    [MaxLength(150)]
    public string Notes { get; set; } = string.Empty;
}