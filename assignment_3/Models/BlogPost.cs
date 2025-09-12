using System.ComponentModel.DataAnnotations;
using assignment_3.Data;

namespace assignment_3.Models;

public class BlogPost
{
    public int Id { get; set; }

    [Required] 
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Summary { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [Required]
    public DateTime Time { get; set; } =  DateTime.Now;
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    
    
    
}