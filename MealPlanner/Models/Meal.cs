using System.ComponentModel.DataAnnotations;
using MealPlanner.Data;

namespace MealPlanner.Models;

public class Meal
{
    public int MealId { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Summary { get; set; }

    public ICollection<MealDishes> Dishes { get; set; } = new List<MealDishes>();

    [Required] 
    public string UserId { get; set; }
    
    public ApplicationUser? User { get; set; }
}