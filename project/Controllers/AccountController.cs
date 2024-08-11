using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using project.Models.ViewModels;
using project.Models;

namespace project.Controllers
{
	public class AccountController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_roleManager = roleManager;
		}

		[HttpPost]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}


     





        //[HttpGet]
        //public IActionResult Login(string? returnUrl = null)
        //{
        //    ViewData["ReturnUrl"] = returnUrl;
        //    return PartialView("_LoginPartial");
        //}







        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    // Redirect to the returnUrl if it's valid, otherwise to the default page
                    var redirectUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action("Index", "Home");
                    return Json(new { redirect = redirectUrl });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }

            // Return the partial view with the model to show validation errors
            return PartialView("_LoginPartial", model);
        }



        public IActionResult Registration()
		{
			return PartialView("_RegistrationPartial", new RegisterViewModel()); // Return empty model for initial load
		}

		[HttpPost]
		public async Task<IActionResult> Registration(RegisterViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
				var result = await _userManager.CreateAsync(user, model.Password);

				if (result.Succeeded)
				{
					if (!await _roleManager.RoleExistsAsync("User"))
					{
						await _roleManager.CreateAsync(new IdentityRole("User"));
					}

					await _userManager.AddToRoleAsync(user, "User");

					return Json(new { redirect = Url.Action("Login", "Account") });
				}

				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}
			return PartialView("_RegistrationPartial", model);
		}

		[HttpGet]
		public IActionResult AssignRole()
		{
			var users = _userManager.Users.ToList();
			return View(users);
		}

		[HttpPost]
		public async Task<IActionResult> AssignRole(string userId, string roleName)
		{
			var user = await _userManager.FindByIdAsync(userId);

			if (user == null)
			{
				ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
				return View("NotFound");
			}

			var roleExists = await _roleManager.RoleExistsAsync(roleName);

			if (!roleExists)
			{
				ViewBag.ErrorMessage = $"Role with Name = {roleName} cannot be found";
				return View("NotFound");
			}

			var isInRole = await _userManager.IsInRoleAsync(user, roleName);

			if (!isInRole)
			{
				var result = await _userManager.AddToRoleAsync(user, roleName);

				if (result.Succeeded)
				{
					return RedirectToAction("Index", "Home");
				}

				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}

			return RedirectToAction("Index", "Home");
		}

		public async Task<IActionResult> UsersWithRoles()
		{
			var usersWithRoles = new List<UserWithRolesViewModel>();

			var users = _userManager.Users.ToList();

			foreach (var user in users)
			{
				var roles = await _userManager.GetRolesAsync(user);

				var userWithRoles = new UserWithRolesViewModel
				{
					UserId = user.Id,
					UserName = user.UserName,
					Roles = roles.ToList()
				};

				usersWithRoles.Add(userWithRoles);
			}

			return View(usersWithRoles);
		}

		public IActionResult AccessDenied()
		{
			return View();
		}
	}
}
