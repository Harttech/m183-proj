using System.Linq;
using System.Text.RegularExpressions;
using Forum.Business;
using Forum.Business.Helpers;
using Forum.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers
{
	public class RegisterController : Controller
	{
		[HttpGet]
		public IActionResult Index()
		{
			return View();
		}

		[ValidateAntiForgeryToken]
		[HttpPost]
		public IActionResult Index(string username, string password, string password2, string phone, string email)
		{
			if (!password.Equals(password2))
			{
				ViewData["PasswordEqual"] = true;
				return View();
			}

			var db = DataBase.Instance;
			var userExist = db.Users.Any(x => x.Username.Equals(username) || x.Email.Equals(email));
			if (userExist)
			{
				ViewData["UserExist"] = true;
				return View();
			}

			if (!Regex.IsMatch(password, Patterns.PassPattern))
			{
				ViewData["PassFormat"] = true;
				return View();
			}

			if (!Regex.IsMatch(phone, Patterns.PhonePattern))
			{
				ViewData["PhoneFormat"] = true;
				return View();
			}

			if (!Regex.IsMatch(email, Patterns.EmailPattern))
			{
				ViewData["EmailFormat"] = true;
				return View();
			}

			phone = phone.Replace("+", "");
			while (Regex.IsMatch(phone, "\\s"))
			{
				phone = Regex.Replace(phone, "\\s", "");
			}

			if (phone.StartsWith("00"))
				phone = phone.Remove(0, 2);

			var user = new User
			{
				Username = username,
				Email = email,
				PhoneNumber = phone,
				Role = Role.MemberNoAuth
			};

			user.Password = CryptoHelper.HashString(password, user.Salt);
			db.Users.Add(user);
			db.SaveChanges();

			HttpContext.Session.SetString("LoggedInUser", user.Id.ToString());
			HttpContext.Session.CommitAsync();
			return RedirectToAction("Auth", "Login");
		}
	}
}