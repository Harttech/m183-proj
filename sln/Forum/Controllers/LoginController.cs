using System;
using System.Linq;
using System.Threading.Tasks;
using Forum.Business;
using Forum.Business.Helpers;
using Forum.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Forum.Controllers
{
	public class LoginController : Controller
	{
		private readonly DataBase _db = DataBase.Instance;
		private User _loggedInUser;

		[HttpGet]
		public IActionResult Index()
		{
			if (!HttpContext.Session.IsAvailable)
				return View("Index");

			var loggedInUser = GetLoggedInUser();
			if (loggedInUser != null)
			{
				var authenticated = IsAuthenticated();
				if (authenticated)
					return RedirectToAction("Index", "Home");

				var firstToken = _db.Tokens.FirstOrDefault(x => x.UserId.Equals(loggedInUser.Id) && DateTime.UtcNow < x.ExpiryUtc);
				if (firstToken == null)
				{
					var token = SmsAuthHelper.RequestToken(loggedInUser.PhoneNumber).GetAwaiter().GetResult();
					var smsToken = new Token
					{
						TokenString = token,
						ExpiryUtc = DateTime.UtcNow.AddMinutes(5),
						UserId = loggedInUser.Id,
						User = loggedInUser
					};
					_db.Tokens.Add(smsToken);
					_db.SaveChanges();
				}
				return View("Auth");
			}
			return View();
		}

		[ValidateAntiForgeryToken]
		[HttpPost]
		public IActionResult Index(string username, string password)
		{
			if (!HttpContext.Session.IsAvailable)
				return View("Index");

			var loggedInUser = GetLoggedInUser();
			if (loggedInUser != null)
			{
				var authenticated = IsAuthenticated();
				if (authenticated)
					return RedirectToAction("Index", "Home");

				var firstToken = _db.Tokens.FirstOrDefault(x => x.UserId.Equals(loggedInUser.Id) && DateTime.UtcNow < x.ExpiryUtc);
				if (firstToken == null)
				{
					var token = SmsAuthHelper.RequestToken(loggedInUser.PhoneNumber).GetAwaiter().GetResult();
					var smsToken = new Token
					{
						TokenString = token,
						ExpiryUtc = DateTime.UtcNow.AddMinutes(5),
						UserId = loggedInUser.Id,
						User = loggedInUser
					};
					_db.Tokens.Add(smsToken);
					_db.SaveChanges();
				}

				return View("Auth");
			}
			else
			{
				var user = _db.Users.SingleOrDefault(x => x.Username.Equals(username));
				if (user == null)
					return View();

				var validPass = CryptoHelper.VerifyHash(password, user.Password);
				if (!validPass)
					return View();

				HttpContext.Session.SetString("LoggedInUser", user.Id.ToString());
				HttpContext.Session.CommitAsync();
				var firstToken = _db.Tokens.FirstOrDefault(x => x.UserId.Equals(user.Id));
				if (firstToken == null)
				{
					var token = SmsAuthHelper.RequestToken(user.PhoneNumber).GetAwaiter().GetResult();
					var smsToken = new Token
					{
						TokenString = token,
						ExpiryUtc = DateTime.UtcNow.AddMinutes(5),
						UserId = user.Id,
						User = user
					};
					_db.Tokens.Add(smsToken);
					_db.SaveChanges();
				}

				return View("Auth");
			}
		}

		[HttpGet]
		public IActionResult Quick(string id)
		{
			User user = null;
			switch (id.ToLowerInvariant())
			{
				case "admin":
					user = _db.Users.FirstOrDefault(x => x.Username.Equals("Admin"));
					break;
					
				case "moderator":
					user = _db.Users.FirstOrDefault(x => x.Username.Equals("Moderator"));
					break;

				default:
					user = _db.Users.FirstOrDefault(x => x.Username.Equals("Member"));
					break;
			}

			if (user != null)
			{
				_loggedInUser = user;
				HttpContext.Session.SetString("LoggedInUser", user.Id.ToString());
				HttpContext.Session.CommitAsync();
				return Auth("DEBUG");
			}

			return Index();
		}

		[HttpGet]
		public async Task<IActionResult> Auth()
		{
			if (!HttpContext.Session.IsAvailable)
				return View("Index");

			var loggedInUser = GetLoggedInUser();
			var authenticated = IsAuthenticated();
			if (loggedInUser == null)
				return View("Index");

			if (authenticated)
				return RedirectToAction("Index", "Dashboard");

			var firstToken = _db.Tokens.FirstOrDefault(x => x.UserId.Equals(loggedInUser.Id) && DateTime.UtcNow < x.ExpiryUtc);
			if (firstToken == null)
			{
				var token = await SmsAuthHelper.RequestToken(loggedInUser.PhoneNumber);
				var smsToken = new Token
				{
					TokenString = token,
					ExpiryUtc = DateTime.UtcNow.AddMinutes(5),
					UserId = loggedInUser.Id,
					User = loggedInUser
				};
				_db.Tokens.Add(smsToken);
				_db.SaveChanges();
			}

			return View();
		}

		[HttpPost]
		public IActionResult Auth(string token)
		{
			if (!HttpContext.Session.IsAvailable)
				return View("Index");

			var loggedInUser = GetLoggedInUser();
			if (loggedInUser == null)
				return View("Index");

#if DEBUG
			if (token.Equals("DEBUG"))
			{
				if (loggedInUser.Role == Role.MemberNoAuth)
					loggedInUser.Role = Role.Member;
				_db.Users.Update(loggedInUser).State = EntityState.Modified;
				_db.SaveChanges();
				HttpContext.Session.SetString("Authenticated", "true");
				HttpContext.Session.CommitAsync();
				return RedirectToAction("Index", "Dashboard");
			}
#endif

			var smsToken = _db.Tokens.FirstOrDefault(x => x.TokenString.Equals(token) && x.UserId.Equals(loggedInUser.Id));

			if (smsToken == null || smsToken.ExpiryUtc < DateTime.UtcNow)
				return View("Index");

			_db.Tokens.Remove(smsToken);

			HttpContext.Session.SetString("Authenticated", "true");
			HttpContext.Session.CommitAsync();
			if (loggedInUser.Role == Role.MemberNoAuth)
				loggedInUser.Role = Role.Member;
			_db.Users.Update(loggedInUser).State = EntityState.Modified;
			_db.SaveChanges();
			return RedirectToAction("Index", "Dashboard");
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