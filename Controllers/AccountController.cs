using InventoryManagement.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Controllers;

[AllowAnonymous]
[Route("account")]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout(string? returnUrl = "/")
    {
        await _signInManager.SignOutAsync();

        if (string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl))
        {
            return Redirect("/");
        }

        return LocalRedirect(returnUrl);
    }
}