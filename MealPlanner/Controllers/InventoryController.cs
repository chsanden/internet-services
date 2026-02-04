using System;
using System.Linq;
using System.Threading.Tasks;
using MealPlanner.Models;
using MealPlanner.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MealPlanner.Controllers;

[Authorize]
public class InventoryController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public InventoryController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;

        var items = await _db.PantryInventory
            .Where(p => p.UserId == userId)
            .Include(p => p.Ingredient)
            .Include(p => p.Unit)
            .OrderBy(p => p.Ingredient.IngredientName)
            .ToListAsync();
        
        // Add a dropdown list for "Add ingredient"
        var existingIds = items.Select(i => i.IngredientId).ToHashSet();
        var options = await _db.Ingredients
            .Where(i => !existingIds.Contains(i.IngredientId))
            .OrderBy(i => i.IngredientName)
            .Select(i => new SelectListItem
            {
                Value = i.IngredientId.ToString(),
                Text = i.IngredientName
            })
            .ToListAsync();
        
        ViewBag.AvailableIngredients = options;
        
        //unit dropdown
        var unitOptions = await _db.Units
            .OrderBy(u => u.UnitId)
            .Select(u => new SelectListItem
            {
                Value = u.UnitId.ToString(),
                Text = u.Name
            })
            .ToListAsync();
        ViewBag.UnitOptions = unitOptions;
            
        return View("Pantry", items);
    }
    
    // POST: /Inventory/Add
    [HttpPost]
    [ValidateAntiForgeryToken]

    public async Task<IActionResult> Add(int ingredientId, decimal? amount, int? unitId)
    {
        var userId = _userManager.GetUserId(User)!;
        
        // Validation
        if(!await _db.Ingredients.AnyAsync(i => i.IngredientId == ingredientId))
            ModelState.AddModelError(nameof(ingredientId), "Ingredient not found");
        
        if(await _db.PantryInventory.FindAsync(userId, ingredientId) != null)
            ModelState.AddModelError(string.Empty, "You already have this ingredient in your pantry");
        
        if (amount is < 0)
            ModelState.AddModelError(nameof(amount), "Amount cannot be negative");
        
        if (!await _db.Units.AnyAsync(u => u.UnitId == unitId))
            ModelState.AddModelError(nameof(unitId), "Unit not found");
        
        if (!ModelState.IsValid)
            return await Index();

        _db.PantryInventory.Add(new PantryInventory
        {
            UserId = userId,
            IngredientId = ingredientId,
            Amount = amount ?? 0m,
            UnitId = unitId,
            LastUpdated = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    
    // POST: Inventory/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int ingredientId, decimal? amount)
    {
        var userId = _userManager.GetUserId(User)!;
        var item = await _db.PantryInventory.FindAsync(userId, ingredientId);
        if (item == null) return NotFound();

        if (amount is < 0)
        {
            ModelState.AddModelError(nameof(amount), "Amount cannot be negative");
            
            item = await _db.PantryInventory.Include(p => p.Ingredient)
                .FirstAsync(p => p.UserId == userId && p.IngredientId == ingredientId);
            return View(item);
        }
        
        item.Amount = amount ?? 0m;
        item.LastUpdated = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    
    // POST: Inventory/Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int ingredientId)
    {
        var userId = _userManager.GetUserId(User)!;
        var item = await _db.PantryInventory.FindAsync(userId, ingredientId);
        if (item != null)
        {
            _db.PantryInventory.Remove(item);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}


