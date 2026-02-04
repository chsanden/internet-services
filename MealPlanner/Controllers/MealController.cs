using System.Security.Claims;
using MealPlanner.Data;
using MealPlanner.Models;
using MealPlanner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Controllers;
[Authorize]
public class MealController : Controller
{
    private readonly ApplicationDbContext _db;
    public MealController(ApplicationDbContext db) => _db = db;
    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet]
    public async Task<ActionResult> Overview()
    {
        var userId = CurrentUserId;
        var meals = await _db.Meals
            .Where(m=>m.UserId == userId)
            .Include(m=> m.Dishes)
            .ThenInclude(md => md.Dish)
            .AsNoTracking()
            .OrderBy(m=> m.Name)
            .ToListAsync();
        
        return View(meals);
    }

    [HttpGet]
    public async Task<ActionResult> Create()
    {
        var vm = new MealEditViewModel
        {
            AllDishes = await _db.Dishes
                .OrderBy(d => d.Name)
                .ToListAsync()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Create(MealEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.AllDishes = await _db.Dishes
                .OrderBy(d => d.Name)
                .ToListAsync();
            return View(vm);
        }

        var meal = new Meal
        {
            Name = vm.Name,
            Summary = vm.Summary,
            UserId = CurrentUserId,
        };
        foreach (var id in vm.SelectedDishIds.Distinct())
        {
            meal.Dishes.Add(new MealDishes { DishId = id });
        }
        _db.Meals.Add(meal);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Overview));
    }

    [HttpGet]
    public async Task<ActionResult> Edit(int id)
    {
        var userId = CurrentUserId;
        var meal = await _db.Meals
            .Where(m => m.UserId == userId)
            .Include(m=>m.Dishes)
            .FirstOrDefaultAsync(m=>m.MealId == id);
        if (meal == null)
            return NotFound();
        var vm = new MealEditViewModel
        {
            MealId = meal.MealId,
            Name = meal.Name,
            Summary = meal.Summary,
            SelectedDishIds = meal.Dishes.Select(md => md.DishId).ToList(),
            AllDishes = await _db.Dishes
                .OrderBy(d => d.Name)
                .ToListAsync()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(MealEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.AllDishes = await _db.Dishes
                .OrderBy(d=>d.Name)
                .ToListAsync();
            return View(vm);
        }
        var userId = CurrentUserId;
        var meal = await _db.Meals
            .Where(m => m.UserId == userId)
            .Include(m=>m.Dishes)
            .FirstOrDefaultAsync(m=> m.MealId == vm.MealId);
        if (meal == null)
            return NotFound();
        meal.Name = vm.Name;
        meal.Summary = vm.Summary;
        var selected = (vm.SelectedDishIds ?? new ()).Distinct().ToHashSet();
        var existingIds = meal.Dishes.Select(MealDishes => MealDishes.DishId).ToHashSet();
        foreach (var md in meal.Dishes.Where(md => !selected.Contains(md.DishId)).ToList())
        {
            meal.Dishes.Remove(md);
        }
        foreach (var id in selected.Where(id=>!existingIds.Contains(id)))
        {
            meal.Dishes.Add(new MealDishes {MealId = meal.MealId, DishId = id});
        }
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Overview));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Delete(int id)
    {
        var userId = CurrentUserId;
        var meal = await  _db.Meals.FindAsync(id);
        if (meal == null)
            return NotFound();
        _db.Meals.Remove(meal);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Overview));
    }
    
}