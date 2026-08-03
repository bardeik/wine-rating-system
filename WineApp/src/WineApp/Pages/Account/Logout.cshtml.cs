using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WineApp.Models;

namespace WineApp.Pages.Account;

[ValidateAntiForgeryToken]
public class LogoutModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LogoutModel(SignInManager<ApplicationUser> signInManager) =>
        _signInManager = signInManager;

    [TempData]
    public string? ReturnUrl { get; set; }

    public IActionResult OnGet(string? returnUrl = null)
    {
        ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Content("~/");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        var safeReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : ReturnUrl;
        return LocalRedirect(Url.IsLocalUrl(safeReturnUrl) ? safeReturnUrl! : Url.Content("~/"));
    }
}
