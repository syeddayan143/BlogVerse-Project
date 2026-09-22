using BlogVerse.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogVerse.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }


        // =====================================================
        // REGISTER - GET
        // =====================================================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        // =====================================================
        // REGISTER - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var existingUser =
                await _userManager.FindByEmailAsync(model.Email);


            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "An account with this email already exists."
                );

                return View(model);
            }


            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name,
                IsActive = true
            };


            var result =
                await _userManager.CreateAsync(
                    user,
                    model.Password
                );


            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(
                    user,
                    isPersistent: false
                );

                TempData["Success"] =
                    "Registration successful!";

                return RedirectToAction(
                    "Index",
                    "Home"
                );
            }


            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description
                );
            }


            return View(model);
        }


        // =====================================================
        // LOGIN - GET
        // =====================================================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        // =====================================================
        // LOGIN - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var user =
                await _userManager.FindByEmailAsync(model.Email);


            // -------------------------------------------------
            // USER NOT FOUND
            // -------------------------------------------------

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email or password."
                );

                return View(model);
            }


            // -------------------------------------------------
            // USER DEACTIVATED
            // -------------------------------------------------

            if (!user.IsActive)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Your account has been deactivated. Please contact the administrator."
                );

                return View(model);
            }


            // -------------------------------------------------
            // LOGIN
            // -------------------------------------------------

            var result =
                await _signInManager.PasswordSignInAsync(
                    user,
                    model.Password,
                    isPersistent: false,
                    lockoutOnFailure: false
                );


            if (result.Succeeded)
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard"
                );
            }


            ModelState.AddModelError(
                string.Empty,
                "Invalid email or password."
            );


            return View(model);
        }


        // =====================================================
        // ACCESS DENIED
        // =====================================================

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }


        // =====================================================
        // LOGOUT
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Login");
        }
    }
}