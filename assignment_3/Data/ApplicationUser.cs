using assignment_3.Models;
using Microsoft.AspNetCore.Identity;
namespace assignment_3.Data;
// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string Nickname { get; set; } = string.Empty;

    public ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
}
