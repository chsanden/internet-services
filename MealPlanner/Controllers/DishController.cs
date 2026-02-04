using System.Security.Claims;
using System.Text.Json;
using MealPlanner.Data;
using MealPlanner.Models;
using MealPlanner.ViewModels;
using MealPlanner.ViewModels.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace MealPlanner.Controllers;



public class DishController : Controller
{
    private readonly ApplicationDbContext _db;
    public DishController(ApplicationDbContext db) => _db = db;
    
    // --- GET /Dish/Overview ---
    public async Task<IActionResult> Overview()
    {
        var dishes = await _db.Dishes
            .Include(d => d.Owner)
            .Include(d => d.DishIngredients)
            .ThenInclude(di => di.Ingredient)
            .Include(d => d.DishIngredients)
            .ThenInclude(di => di.Unit)   
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .ToListAsync();

        return View(dishes);
    }


    
   // --- GET /Dish/Upsert -> Create ---
   
   [HttpGet]
   public async Task<IActionResult> Create()
   {
       DishUpsertViewModel vm;

       if (TempData.TryGetValue("DishDraft", out var draftObj) && draftObj is string draftJson)
       {
           vm = JsonSerializer.Deserialize<DishUpsertViewModel>(draftJson) 
                ?? new DishUpsertViewModel();
       }
       else
       {
           vm = new DishUpsertViewModel();
       }

       await PopulateDropdowns(vm);
       
       ModelState.Clear();

       return View("Upsert", vm);
   }

   // --- GET /Dish/Upsert -> Edit ---
   
   [HttpGet]
   public async Task<IActionResult> Edit(int id)
   {
       var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

       var dish = await _db.Dishes
           .Include(d => d.DishIngredients)
           .ThenInclude(di => di.Ingredient)
           .Include(d => d.DishIngredients)
           .ThenInclude(di => di.Unit)
           .AsNoTracking()
           .FirstOrDefaultAsync(d => d.DishId == id && d.OwnerId == userId);

       if (dish == null)
           return NotFound();

       var vm = new DishUpsertViewModel
       {
           DishId = dish.DishId,
           Name = dish.Name,
           Summary = dish.Summary,
           Description = dish.Description,
           EstimatedTimeMinutes = dish.EstimatedTimeMinutes,
           Ingredients = dish.DishIngredients
               .OrderBy(di => di.Ingredient.IngredientName)
               .Select(di => new DishIngredientRow
               {
                   IngredientId = di.IngredientId,
                   IngredientName = di.Ingredient.IngredientName,
                   Amount = di.Amount,
                   UnitId = di.UnitId
               })
               .ToList(),
           NewIngredient = new DishIngredientRow() // so the "add row" section is blank
       };
       
       await PopulateDropdowns(vm);
       
       ModelState.Clear();
       
       return View("Upsert", vm);
   }

   [HttpGet]
   public IActionResult LoginToSave()
   {
       return RedirectToAction(nameof(Create));
   }
   [HttpPost]
   [ValidateAntiForgeryToken]
   public IActionResult LoginToSave(DishUpsertViewModel vm)
   {
       // user is not logged in (that's the only time user can press this button)
       if (!User.Identity?.IsAuthenticated ?? true)
       {
           TempData["DishDraft"] = JsonSerializer.Serialize(vm);
           return Challenge(); 
       }

       // fallback: if they're somehow already logged in, just go to Upsert->save path
       return RedirectToAction(nameof(Upsert), new { action = "save" });

   }
   
   [HttpPost]
   [ValidateAntiForgeryToken]
   public async Task<IActionResult> Upsert(DishUpsertViewModel vm, string action)
   {
       // handle: user clicked "Login to save"
       if (action == "login")
       {
           if (!User.Identity?.IsAuthenticated ?? true)
           {
               TempData["DishDraft"] = JsonSerializer.Serialize(vm); // stash form-data
               return Challenge(); // triggers login
           }
           action = "save"; // if somehow logged in -> proceed
       }
       
       // handle add

       if (action == "add")
       {
           // Make sure dropdown data is loaded so we can resolve IngredientName
           await PopulateDropdowns(vm);

           if (vm.NewIngredient is { IngredientId: > 0, Amount: { } x, UnitId: > 0 })
           {
               var ingredientName = vm.AllIngredients
                                        .FirstOrDefault(i => i.IngredientId == vm.NewIngredient.IngredientId)?.Name
                                    ?? string.Empty;

               vm.Ingredients.Add(new DishIngredientRow
               {
                   IngredientId   = vm.NewIngredient.IngredientId,
                   IngredientName = ingredientName,
                   Amount         = x,
                   UnitId         = vm.NewIngredient.UnitId,
               });
           }
    
           vm.NewIngredient = new DishIngredientRow(); // clear the new row
           ModelState.Clear();
           return View("Upsert", vm);
       }
       
       // handle: user clicked real "save"
       if (action == "save")
       {
           // just to be safe
           if (!User.Identity?.IsAuthenticated ?? true)
           {
               TempData["DishDraft"] = JsonSerializer.Serialize(vm);
               return Challenge();
           }

           if (!ModelState.IsValid)
           {
               await PopulateDropdowns(vm);
               return View("Upsert", vm);
           }
           
           var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
           
           // handle null values later
           Dish? dish;

           if (vm.DishId == 0)
           {
               // dish creation
               dish = new Dish
               {
                   OwnerId = ownerId,
               };
               _db.Dishes.Add(dish);
           }
           else
           {
               // update existing dish
               dish = await _db.Dishes
                   .Include(d => d.DishIngredients)
                   .FirstOrDefaultAsync(d =>
                       d.DishId == vm.DishId && 
                       d.OwnerId == ownerId);

               if (dish == null)
               {
                   return NotFound();
               }

               dish.DishIngredients.Clear();
           }

           dish.Name = vm.Name?.Trim() ?? string.Empty;
           dish.Summary = string.IsNullOrWhiteSpace(vm.Summary) ? null : vm.Summary.Trim();
           dish.Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim();
           dish.EstimatedTimeMinutes = vm.EstimatedTimeMinutes;
           dish.IsPublic = vm.IsPublic;
           
           if (vm.Ingredients != null)
           {
               var rows = vm.Ingredients
                   .Where(r => r is { IngredientId: > 0, Amount: > 0, UnitId: > 0 })
                   .GroupBy(r => r.IngredientId)
                   .Select(g => g.First())  
                   .ToList();

               foreach (var row in rows)
               {
                   dish.DishIngredients.Add(new DishIngredient
                   {
                       IngredientId = row.IngredientId,
                       Amount       = row.Amount!.Value,
                       UnitId       = row.UnitId!.Value
                   });
               }
           }

           try
           {
               await _db.SaveChangesAsync();
               return RedirectToAction(nameof(Overview));
           }
           catch (DbUpdateException)
           {
               ModelState.AddModelError(string.Empty, "Name exists.");
               await PopulateDropdowns(vm);
               return View("Upsert", vm);
           }
       }
       
       await PopulateDropdowns(vm);
       return View("Upsert", vm);
   }
    
    // private helper that populates dropdowns
    private async Task PopulateDropdowns(DishUpsertViewModel vm)
    {
        vm.AllIngredients = await _db.Ingredients
            .Select(i => new IngredientPick
            {
                IngredientId = i.IngredientId,   // adjust if your PK is named differently
                Name         = i.IngredientName
            })
            .ToListAsync();

        var units = await _db.Units
            .Select(u => new UnitPick
            {
                UnitId = u.UnitId,               // or u.Id, whatever your column is
                Name   = u.Name,
                Symbol = u.Symbol
            })
            .ToListAsync();

        vm.UnitOptions = units
            .Select(u => new SelectListItem
            {
                Value = u.UnitId.ToString(),     // what gets posted
                Text  = u.Display                // "gram (g)"
            })
            .ToList();
    }

}
