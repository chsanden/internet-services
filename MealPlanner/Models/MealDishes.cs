namespace MealPlanner.Models;

public class MealDishes
{
    public int MealId { get; set; }
    public Meal Meal { get; set; } = null!;

    public int DishId { get; set; }
    public Dish Dish { get; set; } = null!;
}