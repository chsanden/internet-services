namespace assignment_3.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using assignment_3.Models;

public class DemoData
{
    public static async Task InitializeDb(IServiceProvider service)
    {
        var um = service.GetService<UserManager<ApplicationUser>>();
        var db = service.GetService<ApplicationDbContext>();

        const string demoPass = "Password1.";

        var alice = await um.FindByEmailAsync("alice@email.com");
        var bob = await um.FindByEmailAsync("bob@email.com");
        var jim = await um.FindByEmailAsync("jim@email.com");
        
        if (alice == null)
        {
            alice = new ApplicationUser
            {
                Nickname = "Alice",
                Email = "alice@email.com",
                UserName = "alice@email.com"
            };
            await um.CreateAsync(alice, demoPass);
        }
        if (bob == null)
        {
            bob = new ApplicationUser
            {
                Nickname = "Bob",
                Email = "bob@email.com",
                UserName = "bob@email.com"
            };
            await um.CreateAsync(bob, demoPass);

        }
        if (jim == null)
        {
            jim = new ApplicationUser
            {
                Nickname = "Jim",
                Email = "jim@email.com",
                UserName = "jim@email.com"
            };
            await um.CreateAsync(jim, demoPass);
        }

        if (!db.BlogPosts.Any())
        {
            db.BlogPosts.AddRange
            (
                new BlogPost
                {
                    Title = "Hello Title",
                    Summary = "Hello Summary",
                    Content = "Hello Content",
                    Time = DateTime.Now.AddDays(-1),
                    UserId = alice.Id
                },
                new BlogPost
                {
                    Title = "Hello Title2",
                    Summary = "Hello Summary2",
                    Content = "Hello Content2",
                    Time = DateTime.Now.AddDays(-2),
                    UserId = bob.Id
                },
                new BlogPost
                {
                    Title = "Hello Title3",
                    Summary = "Hello Summary3",
                    Content = "Hello Content3",
                    Time = DateTime.Now.AddDays(-3),
                    UserId = jim.Id
                }
            );
            await db.SaveChangesAsync();
        }
    }
}