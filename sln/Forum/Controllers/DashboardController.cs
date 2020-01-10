using System;
using System.Linq;
using Forum.Business;
using Forum.Business.Helpers;
using Forum.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers
{
    public class DashboardController : Controller
	{
		private readonly DataBase _db = DataBase.Instance;
		private User _loggedInUser;

		[HttpGet]
		public IActionResult Index()
        {
			var loggedInUser = GetLoggedInUser();
			var authenticated = IsAuthenticated();

			if (loggedInUser == null)
				return RedirectToAction("Logout", "Home");

			ViewData["UserLoggedIn"] = true;
			if (authenticated)
			{
				var posts = _loggedInUser.Role == Role.Administrator ? _db.Posts.ToList() : _loggedInUser.Posts.Where(x => x.Status != Status.Deleted).ToList();
				return View((_loggedInUser, posts).ToTuple());
			}

			return RedirectToAction("Index", "Login");
		}


        private User GetLoggedInUser()
        {
	        var loggedInUser = HttpContext.Session.GetString("LoggedInUser");
	        var notEmpty = loggedInUser.HasText();
	        if (notEmpty)
	        {
		        var guid = Guid.Parse(loggedInUser);
		        if (_loggedInUser == null || _loggedInUser.Id.Equals(guid))
		        {
			        _loggedInUser = _db.Users.FirstOrDefault(x => x.Id.Equals(guid));
		        }

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