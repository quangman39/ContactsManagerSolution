
using ContactsManager.Core.Domain.IndentityEntites;
using ContactsManager.Core.DTO;
using ContactsManager.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ContactsManager.UI.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        //Register
        [Authorize("NotAuthencated")]

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [Authorize("NotAuthencated")]

        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            //Validation

            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return View(registerDTO);
            }
            ApplicationUser user = new ApplicationUser()
            {
                UserName = registerDTO.Email,
                Email = registerDTO.Email,
                PhoneNumber = registerDTO.Phone,
                PersonName = registerDTO.PersonName
            };

            IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password);

            if (result.Succeeded)
            {
                //cheack radio button
                if (registerDTO.UserType == UserTypeOptions.Admin)
                {
                    //create admin role
                    if (await _roleManager.FindByNameAsync(UserTypeOptions.Admin.ToString()) is null)
                    {
                        ApplicationRole applicationRole = new ApplicationRole { Name = UserTypeOptions.Admin.ToString() };
                        await _roleManager.CreateAsync(applicationRole);
                    }

                    //add new user into Role 
                    await _userManager.AddToRoleAsync(user, UserTypeOptions.Admin.ToString());
                }


                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("Register", error.Description);
                }
            }


            return View(registerDTO);
        }

        //Login 
        [HttpGet]
        [Authorize("NotAuthencated")]

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [Authorize("NotAuthencated")]

        public async Task<IActionResult> Login(LoginDTO loginDTO, string? ReturnUrl)
        {
            //Validate
            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return View(loginDTO);
            }

            var result = await _signInManager.PasswordSignInAsync(loginDTO.Email, loginDTO.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                //Check user is admin, isnt it ?
                ApplicationUser? user = await _userManager.FindByEmailAsync(loginDTO.Email);
                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, UserTypeOptions.Admin.ToString()))
                    {
                        return RedirectToAction("Index", "Home", new { area = "Admin" });
                    }
                }

                //redirect to return Url
                if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                {
                    return LocalRedirect(ReturnUrl);

                }
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("Login", "Name or Pasword is Invalid");

            return View(loginDTO);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
