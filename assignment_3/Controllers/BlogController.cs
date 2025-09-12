using assignment_3.Data;
using assignment_3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace assignment_3.Controllers;

public class BlogController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _um;

    public BlogController(UserManager<ApplicationUser> um, ApplicationDbContext db)
    {
        _um = um;
        _db = db;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var posts = await _db.BlogPosts
            .Include(p => p.User)
            .OrderByDescending(p => p.Time)
            .ToListAsync();
        
        return View(posts);
    }
    
    [Authorize]
    [HttpGet]
    public IActionResult Add() => View(new BlogPost());

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(BlogPost input)
    {
        ModelState.Remove(nameof(BlogPost.UserId));
        ModelState.Remove(nameof(BlogPost.User));
        if (!ModelState.IsValid)
            return View(input);

        var userId = _um.GetUserId(User);
        var post = new BlogPost();
        {
            post.Title = input.Title;
            post.Summary = input.Summary;
            post.Content = input.Content;
            post.Time = DateTime.Now;
            post.UserId = userId;
        }
        

        _db.BlogPosts.Add(post);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }


    [Authorize, HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var post = await _db.BlogPosts.FindAsync(id);
        if (post == null)
            return NotFound();
        if (post.UserId != _um.GetUserId(User))
            return Forbid();
        return View(post);
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BlogPost input)
    {
        if (id != input.Id)
            return BadRequest();
        var post = await _db.BlogPosts.FindAsync(id);
        if (post == null)
            return NotFound();
        if  (post.UserId != _um.GetUserId(User))
            return Forbid();
        post.Title = input.Title;
        post.Summary = input.Summary;
        post.Content = input.Content;
        post.Time = DateTime.Now;
        _db.BlogPosts.Update(post);
        await _db.SaveChangesAsync();
        
        return RedirectToAction(nameof(Index));
    }
        
        
}