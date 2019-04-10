using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Cooking.Models.DataModel;
using Cooking.Models.Repository;
using Cooking.Models.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Cooking.Controllers
{
	public class AccountController : Controller
	{
		private readonly UserRepository _userRepository;
		
		public AccountController(UserRepository user_repository)
		{
			_userRepository = user_repository;
		}
		
		[HttpGet]
		public IActionResult Register()
		{
			return View();
		}
		
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterModel register_model)
		{
			if (ModelState.IsValid)
			{
				var user = _userRepository.GetByName(register_model.Name);
				if (user == null)
				{
					// добавляем пользователя в бд
					var user_id = _userRepository.Create(new User{ Name = register_model.Name, Password = register_model.Password });
					user = _userRepository.GetById(user_id);
 
					await Authenticate(user); // аутентификация
 
					return RedirectToAction("Index", "Client");
				}
				
				ModelState.AddModelError("GlobalError", "Пользователь с таким именем уже существует.");
			}
			return View(register_model);
		}
		
		[HttpGet]
		public IActionResult Login()
		{
			return View();
		}
		
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginModel login_model)
		{
			if (ModelState.IsValid)
			{
				var user = _userRepository.GetByName(login_model.Name);
				if (user != null)
				{
					await Authenticate(user); // аутентификация
 
					return RedirectToAction("Index", "Client");
				}
				
				ModelState.AddModelError("GlobalError", "Неверные имя пользователя или пароль.");
			}
			return View(login_model);
		}
		
		private async Task Authenticate(User user)
		{
			// создаем один claim
			var claims = new List<Claim>
			{
				new Claim(ClaimsIdentity.DefaultNameClaimType, user.Name),
			};
			// создаем объект ClaimsIdentity
			var claims_identity = new ClaimsIdentity(claims, "ApplicationCookie");
			// установка аутентификационных куки
			await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claims_identity));
		}
	}
}