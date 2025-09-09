using assignment_2.Models;

namespace assignment_2.Data;

public class ApplicationDbInitializer
{
    public static void Initialize(ApplicationDbContext db)
    {
        db.Database.EnsureDeleted();
        
        db.Database.EnsureCreated();

        var bookings = new[]
        {
            new Booking("Guest 1", "Title 1", "Message1"),
            new Booking("Guest 2", "Title 2", "Message2"),
            new Booking("Guest 3", "Title 3", "Message3"),
            new Booking("Guest 4", "Title 4", "Message4"),
            new Booking("Guest 5", "Title 5", "Message5")
        };
        
        db.Booking.AddRange(bookings);
        
        db.SaveChanges();
    }
}