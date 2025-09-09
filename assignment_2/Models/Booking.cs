using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace assignment_2.Models;

public class Booking

{
    public Booking() {}

    public Booking(string name, string title, string message)
    {
        Name = name;
        Title = title;
        Message = message;
    }
    
    public int Id { get; set; }
    [Required(ErrorMessage = "Name is required")]
    [DisplayName("Name")]
    public string Name { get; set; } = string.Empty;
    [Required(ErrorMessage ="Title is required")]
    [DisplayName("Title")]
    [StringLength(maximumLength: 50, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;
    [Required(ErrorMessage = "Message is required")]
    [DisplayName("Message")]
    [StringLength(maximumLength: 200, MinimumLength = 20)]
    public string Message { get; set; }  = string.Empty;
}