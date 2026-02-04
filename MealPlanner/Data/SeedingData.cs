using MealPlanner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Data;

public class SeedingData
{
    public static async Task SeedAsync(IServiceProvider serviceProvider) 
    {
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var db          = serviceProvider.GetRequiredService<ApplicationDbContext>();

    // Units first
    var gId  = await EnsureUnitAsync(db, "gram",       "g",  UnitKind.Mass,   baseUnitName: "gram",       baseFactor: 1m);
    var kgId = await EnsureUnitAsync(db, "kilogram",   "kg", UnitKind.Mass,   baseUnitName: "gram",       baseFactor: 1000m);
    var mlId = await EnsureUnitAsync(db, "milliliter", "ml", UnitKind.Volume, baseUnitName: "milliliter", baseFactor: 1m);
    var lId  = await EnsureUnitAsync(db, "liter",      "L",  UnitKind.Volume, baseUnitName: "milliliter", baseFactor: 1000m);
    var pcId = await EnsureUnitAsync(db, "piece",      "pcs",UnitKind.Count,  baseUnitName: "piece",      baseFactor: 1m);

    // Ensure roles
    await EnsureRoleAsync(roleManager, "admin");
    await EnsureRoleAsync(roleManager, "user");

    // Ensure demo user
    var user = await userManager.FindByEmailAsync("demo@demo.meal");
    if (user == null)
    {
        user = new ApplicationUser
        { 
            Email          = "demo@demo.meal",
            UserName       = "demo@demo.meal",
            NickName       = "Nickname",
            FirstName      = "FirstName",
            LastName       = "LastName",
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, "Demo123!");
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new Exception($"Failed to create demo user: {errors}");
        }
    }

    // Make sure user is in roles (idempotent)
    if (!await userManager.IsInRoleAsync(user, "admin"))
        await userManager.AddToRoleAsync(user, "admin");
    if (!await userManager.IsInRoleAsync(user, "user"))
        await userManager.AddToRoleAsync(user, "user");

    // Now do your domain seeding
    await EnsureIngredientAsync(db, 1, "Chicken",     pcId);
    await EnsureIngredientAsync(db, 2, "Rice",        gId);
    await EnsureIngredientAsync(db, 3, "Olive Oil",   mlId);
    await EnsureIngredientAsync(db, 4, "Carrot",      pcId);
    await EnsureIngredientAsync(db, 5, "Minced Beef", gId);
    await EnsureIngredientAsync(db, 6, "Pasta",       gId);
    await EnsureIngredientAsync(db, 7, "Salmon",      gId);

    var ingList1 = new List<DishIngredient>
    {
        new DishIngredient { IngredientId = 1, DishId = 1, Amount = 200, UnitId = gId  },
        new DishIngredient { IngredientId = 2, DishId = 1, Amount = 150, UnitId = gId  }
    };
    var ingList2 = new List<DishIngredient>
    {
        new DishIngredient { IngredientId = 2, DishId = 2, Amount = 150, UnitId = gId  },
        new DishIngredient { IngredientId = 7, DishId = 2, Amount = 250, UnitId = gId  },
        new DishIngredient { IngredientId = 3, DishId = 2, Amount = 10,  UnitId = mlId }
    };

    await EnsureDishAsync(db, 1, "Chicken and Rice", null,
        "Boil rice according to package instructions. Cut chicken into desirable size. Season chicken to taste and cook in frying pan. " +
        "Cook chicken for ~8 min at medium - high heat. Plate and serve once cooked.",
        estimatedMinutes: 15, ownerId: user.Id, ingredients: ingList1, mealLinks: null);

    await EnsureDishAsync(db, 2, "Simple Salmon Bowl", "Simple and tasty",
        "Boil rice. Season salmon, drizzle some olive oil and put in air-fryer at 180C for 12 - 15 min. Serve in a deep bowl.",
        estimatedMinutes: 20, ownerId: user.Id, ingredients: ingList2, mealLinks: null);

    await EnsurePantryAsync(db, "demo@demo.meal", ingredientId: 5, amount: 400m, unitId: gId);
    await EnsurePantryAsync(db, "demo@demo.meal", ingredientId: 7, amount: 500m, unitId: gId);
    await EnsurePantryAsync(db, "demo@demo.meal", ingredientId: 2, amount: 600m, unitId: gId);

    await EnsureMealAsync(db, 1, "Quick lunch", "15-20 min prep time",
        new List<MealDishes> { new MealDishes { DishId = 2 } }, user.Id);
    await EnsureMealAsync(db, 2, "Weekend Dinner", "Simple mains for busy days",
        new List<MealDishes> { new MealDishes { DishId = 1 }, new MealDishes { DishId = 2 } },  user.Id);
    }

    
    private static async Task<int> EnsureUnitAsync(
        ApplicationDbContext db, string name, string? symbol, UnitKind kind,
        string baseUnitName, decimal baseFactor)
    {
        var baseUnit = await db.Units.FirstOrDefaultAsync(u => u.Name == baseUnitName);
        if (baseUnit == null)
        {
            baseUnit = new Unit
            {
                Name = baseUnitName,
                Symbol = baseUnitName switch
                {
                    "gram" => "g",
                    "milliliter" => "ml",
                    "piece" => "pcs",
                    _ => null
                },
                Kind = kind,
                BaseFactor = 1m,
                BaseUnitId = null
            };
            db.Units.Add(baseUnit);
            await db.SaveChangesAsync();
        }
        
        var unit = await db.Units.FirstOrDefaultAsync(u => u.Name == name);
        if (unit == null)
        {
            unit = new Unit
            {
                Name = name,
                Symbol = symbol,
                Kind = kind,
                BaseFactor = baseFactor,
                BaseUnitId = baseUnit.UnitId
            };
            db.Units.Add(unit);
            await db.SaveChangesAsync();
        }

        return unit.UnitId;
    }
    
    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    private static async Task EnsureIngredientAsync(ApplicationDbContext db, int ingredientId, string name, int defaultUnitId)
    {
        var item = await db.Ingredients.FindAsync(ingredientId);
        if (item == null)
        {
            db.Ingredients.Add(new Ingredient
            {
                IngredientId = ingredientId,
                IngredientName = name,
                DefaultUnitId = defaultUnitId
            });
        }
        else
        {
            var changed = false;
            if (item.IngredientName != name) { item.IngredientName = name; changed = true; }
            if (item.DefaultUnitId != defaultUnitId) { item.DefaultUnitId = defaultUnitId; changed = true; }
            if (changed) db.Ingredients.Update(item);
        }
        await db.SaveChangesAsync();
    }

    private static async Task EnsureDishAsync(
        ApplicationDbContext db, int id, string name, string? summary,
        string instructions, int estimatedMinutes, string ownerId,
        ICollection<DishIngredient> ingredients, ICollection<MealDishes>? mealLinks)
    {
        var dish = await db.Dishes
            .Include(r => r.DishIngredients)
            .Include(r => r.Meals)
            .FirstOrDefaultAsync(r => r.DishId == id);

        if (dish == null)
        {
            dish = new Dish
            {
                DishId = id,
                Name = name,
                Summary = summary,
                Description = instructions,
                EstimatedTimeMinutes = estimatedMinutes,
                OwnerId = ownerId,
                IsPublic = false
            };

            foreach (var di in ingredients)
            {
                dish.DishIngredients.Add(new DishIngredient
                {
                    IngredientId = di.IngredientId,
                    Amount = di.Amount,
                    UnitId = di.UnitId
                });
            }

            if (mealLinks != null)
            {
                foreach (var link in mealLinks)
                    dish.Meals.Add(new MealDishes { MealId = link.MealId, DishId = id });
            }

            db.Dishes.Add(dish);
        }
        else
        {
            dish.Name = name;
            dish.Summary = summary;
            dish.Description = instructions;
            dish.EstimatedTimeMinutes = estimatedMinutes;

            dish.DishIngredients.Clear();
            foreach (var di in ingredients)
            {
                dish.DishIngredients.Add(new DishIngredient
                {
                    IngredientId = di.IngredientId,
                    Amount = di.Amount,
                    UnitId = di.UnitId
                });
            }

            if (mealLinks != null)
            {
                dish.Meals.Clear();
                foreach (var link in mealLinks)
                    dish.Meals.Add(new MealDishes { MealId = link.MealId, DishId = id });
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task EnsureMealAsync(
        ApplicationDbContext db, int id, string name, string? summary, ICollection<MealDishes> dishes, string ownerId)
    {
        var meal = await db.Meals.Include(m => m.Dishes).FirstOrDefaultAsync(m => m.MealId == id);

        if (meal == null)
        {
            meal = new Meal
            {
                MealId = id,
                Name = name,
                Summary = summary,
                UserId = ownerId
            };

            foreach (var md in dishes)
                meal.Dishes.Add(new MealDishes { DishId = md.DishId });
            db.Meals.Add(meal);
        }
        else
        {
            meal.Name = name;
            meal.Summary = summary;

            meal.Dishes.Clear();
            foreach (var md in dishes)
                meal.Dishes.Add(new MealDishes { DishId = md.DishId, MealId = id });
        }

        await db.SaveChangesAsync();
    }

    private static async Task EnsurePantryAsync(
        ApplicationDbContext db, string userEmail, int ingredientId, decimal amount, int unitId)
    {
        var userId = await db.Users.Where(u => u.Email == userEmail).Select(u => u.Id).SingleOrDefaultAsync()
                    ?? throw new Exception($"Seed error: user '{userEmail}' not found");

        if (!await db.Ingredients.AnyAsync(i => i.IngredientId == ingredientId))
            throw new Exception($"Seed error: Ingredient '{ingredientId}' not found");

        var existing = await db.PantryInventory.FindAsync(userId, ingredientId);
        if (existing == null)
        {
            db.PantryInventory.Add(new PantryInventory
            {
                UserId = userId,
                IngredientId = ingredientId,
                Amount = amount,
                UnitId = unitId,
                LastUpdated = DateTime.UtcNow
            });
        }
        else
        {
            existing.Amount = amount;
            existing.UnitId = unitId;
            existing.LastUpdated = DateTime.UtcNow;
            db.PantryInventory.Update(existing);
        }

        await db.SaveChangesAsync();
    }
}
