using System;
using System.Linq;
using System.Threading.Tasks;
using CinemaTicketSystemCore.Data;
using CinemaTicketSystemCore.Filters;
using CinemaTicketSystemCore.Models;
using CinemaTicketSystemCore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketSystemCore.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _db;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email, // Email is used as username
                    Email = model.Email,
                    EmailConfirmed = true, // Auto-confirm email
                    Name = model.Name,
                    Surname = model.Surname,
                    PhoneNumber = model.PhoneNumber
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                // Filter out duplicate username errors since we only use email
                AddErrorsFiltered(result);
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToLocal(returnUrl);
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return NotFound();
            }

            var user = await _db.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new UserEditViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Surname = user.Surname,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email ?? string.Empty,
                LockVersion = user.LockVersion
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(UserEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _db.Users.FindAsync(model.Id);
                    if (user == null)
                    {
                        return NotFound();
                    }

                    // Check if user is editing their own profile or is admin
                    var currentUserId = _userManager.GetUserId(User);
                    var isAdmin = User.IsInRole("Admin");
                    if (user.Id != currentUserId && !isAdmin)
                    {
                        return Forbid();
                    }

                    // Prevent admins from editing other admins
                    if (isAdmin && user.Id != currentUserId)
                    {
                        var targetIsAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                        if (targetIsAdmin)
                        {
                            return Forbid();
                        }
                    }

                    // Optimistic concurrency check
                    if (user.LockVersion != null && model.LockVersion != null)
                    {
                        if (!user.LockVersion.SequenceEqual(model.LockVersion))
                        {
                            ModelState.AddModelError("", "The record you attempted to edit was modified by another user. Please refresh and try again.");
                            model.LockVersion = user.LockVersion;
                            return View(model);
                        }
                    }

                    user.Name = model.Name;
                    user.Surname = model.Surname;
                    user.PhoneNumber = model.PhoneNumber;
                    // Do NOT allow changing email/username here; they remain as-registered

                    _db.Entry(user).State = EntityState.Modified;
                    await _db.SaveChangesAsync();

                    return RedirectToAction("Index", "Home");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    ModelState.AddModelError("", "The record you attempted to edit was modified by another user. Please refresh and try again.");
                    var entry = ex.Entries.Single();
                    var databaseValues = await entry.GetDatabaseValuesAsync();
                    if (databaseValues != null)
                    {
                        var databaseUser = (ApplicationUser)databaseValues.ToObject();
                        model.LockVersion = databaseUser.LockVersion;
                    }
                    return View(model);
                }
            }
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new UserEditViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Surname = user.Surname,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email ?? string.Empty,
                LockVersion = user.LockVersion
            };

            return View("EditProfile", viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(UserEditViewModel model)
        {
            return await EditProfile(model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private void AddErrorsFiltered(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                // Combine username and email errors into a single email error
                if (error.Code == "DuplicateUserName" || error.Code == "DuplicateEmail")
                {
                    ModelState.AddModelError("Email", "This email address is already registered.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }

    public class RegisterViewModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        [System.ComponentModel.DataAnnotations.Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(100)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(100)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Surname")]
        public string Surname { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.StringLength(20)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
    }

    public class LoginViewModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        [System.ComponentModel.DataAnnotations.Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}

