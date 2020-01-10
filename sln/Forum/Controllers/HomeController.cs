using System;
using System.Linq;
using System.Text.RegularExpressions;
using Forum.Business;
using Forum.Business.Helpers;
using Forum.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers
{
	public class HomeController : Controller
	{
		private readonly DataBase _db = DataBase.Instance;
		private User _loggedInUser;

		[HttpGet]
		public IActionResult Index(bool? delete)
		{
			var db = DataBase.Instance;
			if (delete == true)
			{
				db.Comments.RemoveRange(db.Comments);
				db.Posts.RemoveRange(db.Posts);
				db.Tokens.RemoveRange(db.Tokens);
				db.Users.RemoveRange(db.Users);
				db.SaveChanges();
				return LogOut();
			}

			if (GetLoggedInUser() != null)
			{
				if (IsAuthenticated())
					ViewData["UserLoggedIn"] = true;
				else
					return RedirectToAction("Auth", "Login");
			}
			
			return View(db.Posts.Where(x => x.Status == Status.Published).ToList());
		}

		[HttpGet]
		public IActionResult CreateDefault()
		{
			var db = DataBase.Instance;
			if (db.Users.Any())
				return View("Index");
			return View();
		}

		[HttpPost]
		public IActionResult CreateDefault(string phone)
		{
			if (Regex.IsMatch(phone, Patterns.PhonePattern))
			{
				phone = phone.Replace("+", "");
				while (Regex.IsMatch(phone, "\\s"))
				{
					phone = Regex.Replace(phone, "\\s", "");
				}

				if (phone.StartsWith("00"))
					phone = phone.Remove(0, 2);

				var admin = new User
				{
					Username = "Admin",
					PhoneNumber = phone,
					Email = "does@not.matter",
					Role = Role.Administrator
				};
				var mod = new User
				{
					Username = "Moderator",
					PhoneNumber = phone,
					Email = "does@not.matter",
					Role = Role.Moderator
				};
				var member = new User
				{
					Username = "Member",
					PhoneNumber = phone,
					Email = "does@not.matter",
					Role = Role.Member
				};

				const string pass = "TestPass1!";
				admin.Password = CryptoHelper.HashString(pass, admin.Salt);
				mod.Password = CryptoHelper.HashString(pass, mod.Salt);
				member.Password = CryptoHelper.HashString(pass, member.Salt);
				var db = DataBase.Instance;
				db.Users.Add(admin);
				db.Users.Add(mod);
				db.Users.Add(member);
				db.SaveChanges();
				return View("Index");
			}

			return View();
		}

		[HttpGet]
		public IActionResult LogOut()
		{
			if (HttpContext.Session.IsAvailable)
			{
				HttpContext.Session.Clear();
			}

			return RedirectToAction("Index");
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