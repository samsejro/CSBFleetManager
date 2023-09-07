using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
//using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
//using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
//using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using CSBFleetManager.Entity;
using System.Net.Mail;
using CSBFleetManager.Models;

namespace CSBFleetManager.Controllers
{
    
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
       private readonly ILogger<AccountController> _logger;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
          _logger = logger;
            _emailSender = emailSender;

        }
        public IActionResult Login()
        {
            return View();

        }
        [HttpPost]
        public async Task<IActionResult> Login(InputModel model)
        {
           // returnUrl ??= Url.Content("~/");

            //ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true

                var userName = model.Email;
                if (IsValidEmail(model.Email))
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        userName = user.UserName;
                    }
                }

                var result = await _signInManager.PasswordSignInAsync(userName, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    // var user = await _userManager.FindByEmailAsync(Input.Email);
                    var user = await _userManager.FindByNameAsync(userName);
                    var roles = await _userManager.GetRolesAsync(user);

                    if (roles.Contains("SuperAdmin") || roles.Contains("Admin"))
                    {
                        //return RedirectToAction("Index", "Employee");
                        return RedirectToAction("IndexAdmin", "Home");

                    }
                    //else if (roles.Contains("Admin"))
                    //{
                    //    return RedirectToAction("Index", "Employee");

                    //}
                    else if (roles.Contains("Basic") || roles.Contains("Manager"))
                    {
                        //return RedirectToAction("MDAIndex", "Employee", user.MDAId);
                        return RedirectToAction("IndexManager", "Home", user.MDAId);
                        //returnUrl = "~/Employee/MDAIndex/ Index";
                        //return LocalRedirect();
                    }
                    //else if (roles.Contains("Manager"))
                    //{
                    //    return RedirectToAction("MDAIndex", "Employee", user.MDAId);


                    //}


                    return View();


                }
                
            }

            return View();

        }
        [HttpPost]
        public async Task<IActionResult>Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");

            return RedirectToAction("Login");
        }
        public bool IsValidEmail(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        //public UserRegistrationModel Input { get; set; }
        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Index()
        {
            ////return View();
            //ReturnUrl = returnUrl;
            //ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> OnPostAsync(UserRegistrationModel Input)
        {
            //returnUrl ??= Url.Content("~/");
            //ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                MailAddress address = new MailAddress(Input.Email);
                string userName = address.User;
                var user = new ApplicationUser { UserName = userName, Email = Input.Email, FirstName = Input.FirstName, LastName = Input.LastName };
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    //ensures that newly created users gets the basic role added to thier account
                    await _userManager.AddToRoleAsync(user, Roles.Basic.ToString());

                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    //var callbackUrl = Url.Page(
                    //    "/Account/ConfirmEmail",
                    //    pageHandler: null,
                    //    values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                    //    protocol: Request.Scheme);

                    //await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    //   $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    //if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    //{
                    //    return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    //}
                    //else
                    //{
                    //    await _signInManager.SignInAsync(user, isPersistent: false);
                    //    return LocalRedirect(returnUrl);
                    //}

                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

            }
            return View();
        }

    }
}
