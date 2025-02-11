using IdentityProject.Models;
using IdentityProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace IdentityProject.Controllers
{
    public class IdentityController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailService _emailService;
        public string Message { get; set; } = string.Empty;
        public EmailMFA EmailMFA = new EmailMFA();
        public IdentityController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View();

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                var emailConfirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.ActionLink("ConfirmEmail", "Identity", new { userId = user.Id, token = emailConfirmToken }) ?? "";

                await _emailService.SendEmailAsync("goksemih45@hotmail.com", model.Email, "Confirm your email", confirmationLink);

                return RedirectToAction("Login");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("Register", error.Description);
                }
                return View();
            }
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View();

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, true);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction("LoginTwoFactor", "Identity", new
                    {
                        model.Email,
                        model.RememberMe
                    });
                }
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError("Login", "Account locked out");
                }
                else
                {
                    ModelState.AddModelError("Login", "Invalid login attempt");
                }
                return View();
            }
        }

        public async Task<IActionResult> LoginTwoFactor(string email, bool rememberMe)
        {
            var user = await _userManager.FindByEmailAsync(email);
            
            this.EmailMFA.SecurityCode = string.Empty;
            this.EmailMFA.RememberMe = rememberMe;

            if (user == null)
            {
                ModelState.AddModelError("Login2Factor", "Invalid user");
                return RedirectToAction("Login");
            }

            var securityCode = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
            await _emailService.SendEmailAsync("goksemih45@hotmail.com", email, "Security Code", securityCode);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginTwoFactor(EmailMFA model)
        {
            if (!ModelState.IsValid) return View();
            var result = await _signInManager.TwoFactorSignInAsync("Email", model.SecurityCode, model.RememberMe, model.RememberMe);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError("Login2Factor", "Account locked out");
                }
                else
                {
                    ModelState.AddModelError("Login2Factor", "Invalid login attempt");
                }
                return View();
            }
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    this.Message = "Email confirmed now you can login.";
                    return View((object)Message);
                }
            }
            this.Message = "Email not confirmed";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Identity");
        }
    }
}
