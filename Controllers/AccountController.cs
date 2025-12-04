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
                // Combine duplicate username/email errors into single message (email = username)
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

            // Load user withptimistic concurrency control
            // LockVersion changes on each update, preventing parallel edits from overwriting each other
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
                LockVersion = user.LockVersion  // Send to client for concurrency check
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
                    // Reload user from database to get latest state (prevents stale data in parallel scenarios)
                    // Critical: User may have been modified/deleted by another admin/user between GET and POST
                    var user = await _db.Users.FindAsync(model.Id);
                    if (user == null)
                    {
                        return NotFound();  // User deleted by another process
                    }

                    // Authorization: users can only edit their own profile, admins can edit others
                    var currentUserId = _userManager.GetUserId(User);
                    var isAdmin = User.IsInRole("Admin");
                    if (user.Id != currentUserId && !isAdmin)
                    {
                        return Forbid();
                    }

                    // Prevent admins from editing other admins (security restriction)
                    if (isAdmin && user.Id != currentUserId)
                    {
                        var targetIsAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                        if (targetIsAdmin)
                        {
                            return Forbid();
                        }
                    }

                    // Phase 1: Client-side optimistic concurrency check (before DB write)
                    // Compare LockVersion from form submission with current DB value
                    // If different, another user/admin modified this record concurrently
                    if (user.LockVersion != null && model.LockVersion != null)
                    {
                        if (!user.LockVersion.SequenceEqual(model.LockVersion))
                        {
                            // Parallel edit detected: show error and refresh with latest data
                            ModelState.AddModelError("", "The record you attempted to edit was modified by another user. Please refresh and try again.");
                            model.LockVersion = user.LockVersion;
                            return View(model);
                        }
                    }

                    // Update user fields (email/username cannot be changed)
                    user.Name = model.Name;
                    user.Surname = model.Surname;
                    user.PhoneNumber = model.PhoneNumber;

                    _db.Entry(user).State = EntityState.Modified;
                    // Phase 2: Database-level concurrency check during SaveChanges
                    // EF Core will throw DbUpdateConcurrencyException if LockVersion changed
                    await _db.SaveChangesAsync();

                    TempData["SuccessMessage"] = "User profile updated successfully.";
                    
                    // If admin is editing another user, redirect to users list; otherwise back to edit profile
                    if (isAdmin && user.Id != currentUserId)
                    {
                        return RedirectToAction("UsersList", "Admin");
                    }
                    return RedirectToAction("EditProfile");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    // Parallel edit detected at database level (race condition between check and save)
                    // Another user/admin saved changes between our LockVersion check and SaveChanges
                    // Reload current database values to show user the latest data
                    ModelState.AddModelError("", "The record you attempted to edit was modified by another user. Please refresh and try again.");
                    var entry = ex.Entries.Single();
                    var databaseValues = await entry.GetDatabaseValuesAsync();
                    if (databaseValues != null)
                    {
                        var databaseUser = (ApplicationUser)databaseValues.ToObject();
                        model.LockVersion = databaseUser.LockVersion;  // Update with latest LockVersion
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
            // Admin editing another user: load with LockVersion for concurrency protection
            // Multiple admins can edit users simultaneously, so concurrency checks are critical
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
                LockVersion = user.LockVersion  // Required for parallel edit detection
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

        // Combines duplicate username/email errors (email = username in this app)
        private void AddErrorsFiltered(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
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

