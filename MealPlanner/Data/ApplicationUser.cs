using System.ComponentModel.DataAnnotations;
using MealPlanner.Models;
using Microsoft.AspNetCore.Identity;

namespace MealPlanner.Data;

public class ApplicationUser : IdentityUser
{
    [Required]
    public string NickName { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    
    public  ICollection<Dish> Dishes { get; set; } = new List<Dish>();
    public ICollection<PantryInventory> Ingredients { get; set; } = new List<PantryInventory>();
}
