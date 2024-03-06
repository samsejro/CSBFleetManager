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
using Microsoft.AspNetCore.Mvc.Rendering;
using CSBFleetManager.Services;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace CSBFleetManager.Controllers
{
    
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
       private readonly ILogger<AccountController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IMDAService _mdaService;

        [TempData]
        public string StatusMessage { get; set; }
        [TempData]
        public string UserNameChangeLimitMessage { get; set; }
        public string Username { get; set; }
        [BindProperty]
        public ManageProfileModel Input { get; set; }

        public List<SelectListItem> MDAList { get; set; }
        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, IMDAService mDAService, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
          _logger = logger;
            _emailSender = emailSender;
            _mdaService = mDAService;

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
                        ViewBag.MDAId = user.MDAId;
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

            //return RedirectToAction("Login");
            return RedirectToAction("NewLogin");
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
        public async Task<IActionResult> NewLogin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> NewLogin(InputModel model)
        {
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

        public async Task<IActionResult> Register()
        {
            MDAList = _mdaService.GetAllMDAforEmployee().ToList();
            ViewBag.MDAList= _mdaService.GetAllMDAforEmployee().ToList();

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegistrationModel model)
        {
            MDAList = _mdaService.GetAllMDAforEmployee().ToList();
            ViewBag.MDAList = _mdaService.GetAllMDAforEmployee().ToList();

            if (ModelState.IsValid)
            {
                MailAddress address = new MailAddress(model.Email);
                string userName = address.User;
                var user = new ApplicationUser { UserName = userName, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName, MDAId = model.MDAId };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

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
                    //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    //if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    //{
                    //    return RedirectToPage("RegisterConfirmation", new { email = model.Email, returnUrl = returnUrl });
                    //}
                    //else
                    //{
                    //    await _signInManager.SignInAsync(user, isPersistent: false);
                    //    return LocalRedirect(returnUrl);
                    //}
                    TempData["Success"] = "User Created Successfully";
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View();
        }

        public async Task LoadUserAsync(ApplicationUser user)
        {

            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            var firstName = user.FirstName;
            var lastName = user.LastName;
            var profilePicture = user.ProfilePicture;

            Username = userName;

            Input = new ManageProfileModel
            {
                PhoneNumber = phoneNumber,
                Username = userName,
                FirstName = firstName,
                LastName = lastName,
                ProfilePicture = profilePicture
            };

            
        }
        
        //Get Method
        public async Task<IActionResult> ManageUserIndex()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            UserNameChangeLimitMessage = $"You can change your username {user.UsernameChangeLimit} more time(s).";
            ViewBag.UserNameChangeLimitMessage = UserNameChangeLimitMessage;
            //await LoadUserAsync(user);

            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var firstName = user.FirstName;
            var lastName = user.LastName;
            var profilePicture = user.ProfilePicture;

            Username = userName;

            Input = new ManageProfileModel
            {
                PhoneNumber = phoneNumber,
                Username = userName,
                FirstName = firstName,
                LastName = lastName,
                ProfilePicture = profilePicture
            };

            if (roles.Contains("SuperAdmin") || roles.Contains("Admin"))
            {
                //return RedirectToAction("Index", "Employee");
                ViewData["Layout"] = "~/Views/Shared/AdminLTE/_Layout.cshtml";

            }
            
            else if (roles.Contains("Basic") || roles.Contains("Manager"))
            {
                ViewData["Layout"] = "~/Views/Shared/AdminLTE/_LayoutManager.cshtml";
            }

            return View(Input);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserIndex(ManageProfileModel model)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var firstName = user.FirstName;
            var lastName = user.LastName;

            if (!ModelState.IsValid)
            {
                await LoadUserAsync(user);
                return View();
            }
            // check altered values in fields

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (model.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    //return RedirectToPage();
                    //return RedirectToAction("OnGetAsync");
                    return RedirectToAction("ManageUserIndex");
                }
                else
                {
                    TempData["Success"] = "Phone number updated Successfully";
                }
            }
            if (model.FirstName != firstName)
            {
                user.FirstName = model.FirstName;
                await _userManager.UpdateAsync(user);
            }
            if (model.LastName != lastName)
            {
                user.LastName = model.LastName;
                await _userManager.UpdateAsync(user);
            }
            //Check the number of times user can change their username
            if (user.UsernameChangeLimit > 0)
            {
                if (model.Username != user.UserName)
                {
                    var userNameExists = await _userManager.FindByNameAsync(model.Username);
                    if (userNameExists != null)
                    {
                        StatusMessage = "User name already taken. Select a different username.";
                        //return RedirectToPage();
                        //return RedirectToAction("OnGetAsync");
                        TempData["Error"] = "User name already taken. Select a different username";
                        return RedirectToAction("ManageUserIndex");
                    }
                    var setUserName = await _userManager.SetUserNameAsync(user, model.Username);
                    if (!setUserName.Succeeded)
                    {
                        StatusMessage = "Unexpected error when trying to set user name.";
                        //return RedirectToPage();
                        //return RedirectToAction("OnGetAsync");
                        return RedirectToAction("ManageUserIndex");
                    }
                    else
                    {
                        user.UsernameChangeLimit -= 1;
                        await _userManager.UpdateAsync(user);
                        TempData["Success"] = "User details updated Successfully";
                    }
                }
            }

            if (Request.Form.Files.Count > 0)
            {
                IFormFile file = Request.Form.Files.FirstOrDefault();
                using (var dataStream = new MemoryStream())
                {
                    await file.CopyToAsync(dataStream);
                    user.ProfilePicture = dataStream.ToArray();
                }
                await _userManager.UpdateAsync(user);
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";


            ViewBag.StatusMessage = StatusMessage;
            TempData["Success"] = "Your profile has been updated Successfully";
            return RedirectToAction("ManageUserIndex");
        }


        public async Task<IActionResult> USerPasswordChange()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            var hasPassword = await _userManager.HasPasswordAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("SuperAdmin") || roles.Contains("Admin"))
            {
                //return RedirectToAction("Index", "Employee");
                ViewData["Layout"] = "~/Views/Shared/AdminLTE/_Layout.cshtml";

            }

            else if (roles.Contains("Basic") || roles.Contains("Manager"))
            {
                ViewData["Layout"] = "~/Views/Shared/AdminLTE/_LayoutManager.cshtml";
            }

            if (!hasPassword)
            {
                return View();
            }


            return View();
        }
        [HttpPost]
        public async Task<IActionResult> USerPasswordChange(UserPasswordChangeViewModel model)
        {

            if (!ModelState.IsValid)
            {
                StatusMessage = "Unexpected error when trying to set user name.";
                return View();
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                StatusMessage = "Unexpected error. Please try again.";
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    
                }
                StatusMessage = "Password updated not successful";

                TempData["Error"] = "Password updated not successful";

                return View();
            }
            //StatusMessage = "Password updated successfully";
           
            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User changed their password successfully.");
            StatusMessage = "Your password has been changed.";
            ViewBag.StatusMessage = StatusMessage;
            TempData["Success"] = "Your password has been changed successfully";
            return View();
           // return RedirectToAction(nameof(NewLogin));
        }


    }
    
}
