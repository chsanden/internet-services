using MealPlanner.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace MealPlanner.ViewModels;

public class MealEditViewModel
{
    public int?  MealId { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string? Summary { get; set; }

    public List<int> SelectedDishIds { get; set; } = new();

    public List<Dish> AllDishes { get; set; } = new();
}