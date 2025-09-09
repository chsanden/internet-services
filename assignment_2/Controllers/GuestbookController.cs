using assignment_2.Data;
using assignment_2.Models;
using Microsoft.AspNetCore.Mvc;

namespace assignment_2.Controllers;

public class GuestbookController : Controller
{
    private readonly ApplicationDbContext _db;

    public GuestbookController(ApplicationDbContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        var bookings = _db.Booking.ToList();
        
        return View(bookings);
    }

    [HttpGet]
    public IActionResult Add()
    {
        return View(new Booking());
    }

    [HttpPost]
    public IActionResult Add(Booking booking)
    {
        if(!ModelState.IsValid)
            return View(booking);
        
        _db.Booking.Add(booking);
        _db.SaveChanges();
        
        return RedirectToAction(nameof(Index));
    }
    
}