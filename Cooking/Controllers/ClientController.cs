using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cooking.Controllers
{
	[Authorize]
	public class ClientController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}