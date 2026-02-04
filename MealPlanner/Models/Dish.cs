using System.ComponentModel.DataAnnotations;
using MealPlanner.Data;

namespace MealPlanner.Models;

public class Dish
{
    public int DishId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Summary { get; set; }

    [MaxLength(600)]
    public string? Description { get; set; }

    public int EstimatedTimeMinutes { get; set; }
    
    public bool IsPublic { get; set; }

    // Ownership
    [Required]
    public string OwnerId { get; set; } = string.Empty;
    public ApplicationUser Owner { get; set; } = null!;

    // Navigation
    public ICollection<DishIngredient> DishIngredients { get; set; } = new List<DishIngredient>();
    public ICollection<MealDishes> Meals { get; set; } = new List<MealDishes>();
}