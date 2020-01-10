using System;
using System.Linq;
using Forum.Business;
using Forum.Business.Helpers;
using Forum.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Forum.Controllers
{
	public class PostController : Controller
	{
		private readonly DataBase _db = DataBase.Instance;
		private User _loggedInUser;

		[HttpGet]
		public IActionResult Index()
		{
			return RedirectToAction("Index", "Dashboard");
		}

		[HttpGet]
		public IActionResult Read(string id)
		{
			if (id.HasText())
			{
				if (Guid.TryParse(id, out var guid))
				{
					var post = _db.Posts.FirstOrDefault(x => x.Id.Equals(guid));
					return View("Read", (GetLoggedInUser(), post).ToTuple());
				}
			}
			return View("Read", (GetLoggedInUser(), default(Post)).ToTuple());
		}

		[HttpGet]
		public IActionResult Delete(string id)
		{
			var loggedInUser = GetLoggedInUser();

			if (loggedInUser != null && IsAuthenticated())
			{
				if (Guid.TryParse(id, out var guid))
				{
					var post = _db.Posts.FirstOrDefault(x => x.Id.Equals(guid));
					if (post != null)
					{
						if (loggedInUser.Role == Role.Administrator)
						{
							_db.Posts.Remove(post);
						}
						else if (post.UserId.Equals(loggedInUser.Id))
						{
							post.Status = Status.Deleted;
							_db.Posts.Update(post).State = EntityState.Modified;
						}
					}
				}
			}
			return Index();
		}

		[HttpGet]
		public IActionResult Publish(string id)
		{
			var loggedInUser = GetLoggedInUser();

			if (loggedInUser != null && IsAuthenticated())
			{
				if (Guid.TryParse(id, out var guid))
				{
					var post = _db.Posts.FirstOrDefault(x => x.Id.Equals(guid));
					if (post != null)
					{
						if (loggedInUser.Role == Role.Administrator)
						{
							post.Status = Status.Published;
							var update = _db.Posts.Update(post);
							update.State = EntityState.Modified;
							_db.SaveChanges();
						}
					}
				}
			}
			return Index();
		}

		[HttpGet]
		public IActionResult Hide(string id)
		{
			var loggedInUser = GetLoggedInUser();

			if (loggedInUser != null && IsAuthenticated())
			{
				if (Guid.TryParse(id, out var guid))
				{
					var post = _db.Posts.FirstOrDefault(x => x.Id.Equals(guid));
					if (post != null)
					{
						if (loggedInUser.Role == Role.Administrator)
						{
							post.Status = Status.Hidden;
							var update = _db.Posts.Update(post);
							update.State = EntityState.Modified;
							_db.SaveChanges();
						}
					}
				}
			}
			return Index();
		}

		[HttpGet]
		public IActionResult New()
		{
			var invalid = RedirectToHomeIfInvalidLogin();
			if (invalid != null)
				return invalid;
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult New(Post post)
		{
			var invalid = RedirectToHomeIfInvalidLogin();
			if (invalid != null)
				return invalid;

			if (post.Title.NullOrWhiteSpace())
			{
				ViewData["Error"] = "TitleNull";
				return View();
			}

			if (post.Content.NullOrWhiteSpace())
			{
				ViewData["Error"] = "ContentNull";
				return View();
			}

			if (post.Content.Length > 200)
			{
				ViewData["Error"] = "ContentTooLong";
				return View();
			}

			var user = GetLoggedInUser();
			post.UserId = user.Id;
			post.User = user;

			_db.Posts.Add(post);
			_db.SaveChanges();

			return RedirectToAction("Index", "Dashboard");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Comment(string postId, string comment)
		{
			if (comment.Length > 200)
				return RedirectToAction("Read", new { id = postId });

			var loggedInUser = GetLoggedInUser();
			if (loggedInUser != null)
			{
				if (!IsAuthenticated() || loggedInUser.Role == Role.MemberNoAuth)
					return RedirectToAction("Auth", "Login");

				if (Guid.TryParse(postId, out var guid))
				{
					var post = _db.Posts.FirstOrDefault(x => x.Id.Equals(guid));
					if (post == null)
						return Read(postId);

					var newComment = new Comment
					{
						User = loggedInUser,
						UserId = loggedInUser.Id,
						Post = post,
						PostId = post.Id,
						Content = comment
					};

					_db.Comments.Add(newComment);
					_db.SaveChanges();
				}
			}
			return RedirectToAction("Read", new { id = postId });
		}

		private IActionResult RedirectToHomeIfInvalidLogin()
		{
			var authenticated = IsAuthenticated();
			var loggedInUser = GetLoggedInUser();
			if (loggedInUser == null)
			{
				if (authenticated)
					return RedirectToAction("Logout", "Home");
				return RedirectToAction("Index", "Login");
			}
			return null;
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