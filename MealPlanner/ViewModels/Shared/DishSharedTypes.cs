using System.ComponentModel.DataAnnotations;

namespace MealPlanner.ViewModels.Shared
{
    public class DishIngredientRow
    {
        [Required]
        public int IngredientId { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        [Range(0.0, 1_000_000.0, ErrorMessage = "Amount must be positive.")]
        public decimal? Amount { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Please select a unit.")]
        public int? UnitId { get; set; }
        public bool Remove { get; set; }
    }

    public class IngredientPick
    {
        public int IngredientId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class UnitPick
    {
        public int UnitId { get; set; }
        public string Name { get; set; } = string.Empty;   // e.g., "gram"
        public string? Symbol { get; set; }                // e.g., "g"
        public string Display => string.IsNullOrWhiteSpace(Symbol) ? Name : $"{Name} ({Symbol})";
    }

    public class RecipePick
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}