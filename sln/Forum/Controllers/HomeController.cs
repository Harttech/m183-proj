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
				return RedirectToAction("Index");
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

				var adminPost = new Post
				{
					Title = "Admin credentials",
					Content = "User: Admin\r\nPass: TestPass1!",
					User = admin,
					UserId = admin.Id,
					Status = Status.Published
				};

				var userPost1 = new Post
				{
					Title = "User credentials",
					Content = "User: Member\r\nPass: TestPass1!",
					User = member,
					UserId = member.Id,
					Status = Status.Published
				};

				var userPost2 = new Post
				{
					Title = "User post deleted",
					Content = "This post was deleted by a user and is only visible for the administrator. They can restore it or delete it permanently.",
					User = member,
					UserId = member.Id,
					Status = Status.Deleted
				};

				var userPost3 = new Post
				{
					Title = "User post hidden",
					Content = "This post was created by a user. But it has to be published by an admin. Until then it will be hidden and only viewable to admins and the creator.",
					User = member,
					UserId = member.Id,
					Status = Status.Hidden
				};

				db.Posts.Add(adminPost);
				db.Posts.Add(userPost1);
				db.Posts.Add(userPost2);
				db.Posts.Add(userPost3);

				var userComment = new Comment
				{
					Content = "You DO know this post is public, right?",
					User = member,
					UserId = member.Id,
					Post = adminPost,
					PostId = adminPost.Id
				};

				var adminComment = new Comment
				{
					Content = "Oh crap.",
					User = admin,
					UserId = admin.Id,
					Post = adminPost,
					PostId = adminPost.Id
				};

				var adminComment2 = new Comment
				{
					Content = "Same counts for you too. Hide your post, idiot.",
					User = admin,
					UserId = admin.Id,
					Post = userPost1,
					PostId = userPost1.Id
				};

				var userComment2 = new Comment
				{
					Content = "Actually this is intentionally. Btw, did you know you can use \"DEBUG\" as auth code to login faster?",
					User = member,
					UserId = member.Id,
					Post = userPost1,
					PostId = userPost1.Id
				};

				var adminComment3 = new Comment
				{
					Content = "Yes...I am the admin. Of course I know.",
					User = admin,
					UserId = admin.Id,
					Post = userPost1,
					PostId = userPost1.Id
				};

				var userComment3 = new Comment
				{
					Content = "Oh, right.",
					User = member,
					UserId = member.Id,
					Post = userPost1,
					PostId = userPost1.Id
				};

				var modComment = new Comment
				{
					Content = "I should rather work on more important systems than writing these comments.",
					User = mod,
					UserId = mod.Id,
					Post = userPost1,
					PostId = userPost1.Id
				};

				db.Comments.Add(userComment);
				db.Comments.Add(adminComment);
				db.Comments.Add(adminComment2);
				db.Comments.Add(userComment2);
				db.Comments.Add(adminComment3);
				db.Comments.Add(userComment3);
				db.Comments.Add(modComment);

				db.SaveChanges();
			}

			return RedirectToAction("Index");
		}

		[HttpGet]
		public IActionResult LogOut()
		{
			if (HttpContext.Session.IsAvailable)
			{
				HttpContext.Session.Clear();
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