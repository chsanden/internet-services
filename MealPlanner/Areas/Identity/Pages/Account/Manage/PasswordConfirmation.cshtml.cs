using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MealPlanner.Data;

namespace MealPlanner.Areas.Identity.Pages.Account.Manage
{
    public class PasswordConfirmationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public PasswordConfirmationModel(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty] 
        public string Password { get; set; } = string.Empty;

        [TempData] 
        public string? StatusMessage { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }
        
        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();
        
        public class InputModel
        {
            public string? NickName { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? NewEmail { get; set; }
            public string? PhoneNumber { get; set; }
        }

        public IActionResult OnGet()
        {
            if (TempData["ProfileChanges"] == null)
            {
                return RedirectToPage("Index");
            }
            var changes = JsonSerializer.Deserialize<InputModel>((string)TempData["ProfileChanges"]!);
            if (changes != null)
            {
                Input.NickName = changes.NickName;
                Input.FirstName = changes.FirstName;
                Input.LastName = changes.LastName;
                Input.NewEmail = changes.NewEmail;
                Input.PhoneNumber = changes.PhoneNumber;
            }
            TempData.Keep("ProfileChanges"); 
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var validPassword = await _userManager.CheckPasswordAsync(user, Password);
            if (!validPassword)
            {
                StatusMessage = "Wrong password";
                return RedirectToPage("Index");
            }

            if (!string.IsNullOrWhiteSpace(Input.NickName)) user.NickName = Input.NickName;
            if (!string.IsNullOrWhiteSpace(Input.FirstName)) user.FirstName = Input.FirstName;
            if (!string.IsNullOrWhiteSpace(Input.LastName)) user.LastName = Input.LastName;
            if (!string.IsNullOrWhiteSpace(Input.NewEmail)) user.Email = Input.NewEmail;
            if (!string.IsNullOrWhiteSpace(Input.NewEmail)) user.UserName = Input.NewEmail;
            if (!string.IsNullOrWhiteSpace(Input.PhoneNumber))
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }
            
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                StatusMessage = "Error updating user information";
                return RedirectToPage("Index");
            }
            
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "User information has been updated";
            
            return Redirect(ReturnUrl);
        }
    }
}