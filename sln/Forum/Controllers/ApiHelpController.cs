using System;
using System.Linq;
using Forum.Business.Helpers;
using Forum.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers
{
	public class ApiHelpController : Controller
	{
		private readonly DataBase _db = DataBase.Instance;
		private User _loggedInUser;

		public IActionResult Index()
		{
			var loggedInUser = GetLoggedInUser();
			if (loggedInUser != null && IsAuthenticated())
				return View(loggedInUser);
			return View();
		}

		[HttpGet]
		public IActionResult GetApiKey()
		{
			var loggedInUser = GetLoggedInUser();
			if (loggedInUser != null)
			{
				if (IsAuthenticated())
				{
					ApiController.GetToken(loggedInUser);
					return RedirectToAction("Index");
				}
				return Unauthorized();
			}
			return BadRequest();
		}

		private User GetLoggedInUser()
		{
			var loggedInUser = HttpContext.Session.GetString("LoggedInUser");
			var notEmpty = loggedInUser.HasText();
			if (notEmpty)
			{
				var guid = Guid.Parse(loggedInUser);
				if (_loggedInUser == null || _loggedInUser.Id.Equals(guid))
					_loggedInUser = _db.Users.FirstOrDefault(x => x.Id.Equals(guid));

				ViewData["UserLoggedIn"] = _loggedInUser != null;
				return _loggedInUser;
			}
			ViewData["UserLoggedIn"] = false;
			return null;
		}

		private bool IsAuthenticated()
		{
			var authenticated = HttpContext.Session.GetString("Authenticated");
			return authenticated.HasText() && authenticated.ToLowerInvariant().Equals("true");
		}
	}
}