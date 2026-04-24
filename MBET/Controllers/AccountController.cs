using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MBET.Core.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using MBET.Shared.Resources;

namespace MBET.web.Controllers
{
    [Route("account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IStringLocalizer<L> _l;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IStringLocalizer<L> l)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _l = l;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] LoginViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Redirect($"/login?error={Uri.EscapeDataString(_l["InvalidCredentials"])}");
            }

            // NEW: Banned Check
            if (user.IsBanned)
            {
                return Redirect($"/login?error={Uri.EscapeDataString(_l["AccountBanned"])}");
            }

            // Check if user is active (Approval Workflow)
            if (!user.IsActive)
            {
                return Redirect($"/login?error={Uri.EscapeDataString(_l["AccountPendingApproval"])}");
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                return Redirect("/");
            }
            if (result.IsLockedOut)
            {
                return Redirect($"/login?error={Uri.EscapeDataString(_l["AccountLockedOut"])}");
            }

            return Redirect($"/login?error={Uri.EscapeDataString(_l["InvalidCredentials"])}");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterViewModel model)
        {
            if (model.Password != model.ConfirmPassword)
            {
                return Redirect($"/register?error={Uri.EscapeDataString(_l["PasswordsDoNotMatch"])}");
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                IsActive = true, // Auto-activate for now
                CreatedAt = DateTime.UtcNow,
                TenantId = Guid.NewGuid()
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign default Customer role
                await _userManager.AddToRoleAsync(user, "Customer");
                return Redirect($"/login?message={Uri.EscapeDataString(_l["AccountCreatedSuccess"])}");
            }

            // Framework automatically localizes Identity errors. 
            // We join them with a pipe "|" so the Razor component can split them into a clean bulleted list.
            string errors = string.Join(" | ", result.Errors.Select(e => e.Description));
            return Redirect($"/register?error={Uri.EscapeDataString(errors)}");
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Redirect("/login");
        }
    }

    public class LoginViewModel
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class RegisterViewModel
    {
        [Required]
        public string FirstName { get; set; } = "";

        [Required]
        public string LastName { get; set; } = "";

        [Required]
        public string Email { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";

        public string ConfirmPassword { get; set; } = "";
    }
}