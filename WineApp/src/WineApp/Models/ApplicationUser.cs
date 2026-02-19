using Microsoft.AspNetCore.Identity;

namespace WineApp.Models;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}
