using System.ComponentModel.DataAnnotations;

namespace MealPlanner.Models;

public enum UnitKind
{
    Mass,    // g, kg
    Volume,  // ml, L, cup, tbsp, tsp
    Count,   // piece, each
    Other
}

public class Unit
{
    public int UnitId { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(10)]
    public string? Symbol { get; set; }

    public UnitKind Kind { get; set; } = UnitKind.Other;

    public decimal BaseFactor { get; set; } = 1m;
    
    public int? BaseUnitId { get; set; }
    public Unit? BaseUnit { get; set; }
}