using System.ComponentModel.DataAnnotations;
using MealPlanner.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MealPlanner.ViewModels
{
    public class DishUpsertViewModel
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

        // What the form binds to
        public List<DishIngredientRow> Ingredients { get; set; } = new();
        
        [ValidateNever]
        public DishIngredientRow NewIngredient { get; set; } = new();

        // Pick lists
        public List<IngredientPick> AllIngredients { get; set; } = new();
        public List<SelectListItem> UnitOptions { get; set; } = new();
    }

}