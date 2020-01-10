using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Forum.Business;
using Forum.Business.Helpers;
using Forum.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class ApiController : ControllerBase
	{
		private readonly DataBase _db = DataBase.Instance;

		private static string GetServerSecret()
		{
			if (System.IO.File.Exists("Secret.txt"))
				return System.IO.File.ReadAllText("Secret.txt");
			return null;
		}

		private static string GetApiKey(string userSecret)
		{
			var serverSecret = GetServerSecret();
			if (serverSecret.HasText())
			{
				var apiKey = CryptoHelper.HashString(serverSecret, userSecret);
				return apiKey;
			}
			return null;
		}

		private bool AuthenticateKey(string apiKey, out User user)
		{
			user = null;

			if (apiKey.NullOrWhiteSpace())
				return false;

			var userSecret = CryptoHelper.ExtractSalt(apiKey);
			var token = _db.Tokens.FirstOrDefault(x => x.TokenKind == TokenKind.ApiAuthToken && DateTime.UtcNow < x.ExpiryUtc && x.TokenString.Equals(userSecret));
			if (token == null)
				return false;

			user = token.User;
			return CryptoHelper.VerifyHash(GetServerSecret(), apiKey);
		}

		private string GetErrorResponse(string status, string response)
		{
			var obj = new { error = status, reason = response };
			return JsonSerializer.Serialize(obj);
		}

		internal static string GetToken(User user)
		{
			var db = DataBase.Instance;
			if (user != null)
			{
				if (user.Role == Role.MemberNoAuth)
					return null;

				string tokenString;
				var first = db.Tokens.FirstOrDefault(x => x.TokenKind == TokenKind.ApiAuthToken && DateTime.UtcNow < x.ExpiryUtc && x.UserId.Equals(user.Id));

				if (first == null)
				{
					var token = new Token
					{
						ExpiryUtc = DateTime.MaxValue,
						TokenKind = TokenKind.ApiAuthToken,
						UserId = user.Id,
						TokenString = CryptoHelper.GenerateRandomSalt()
					};

					db.Tokens.Add(token);
					db.SaveChanges();
					tokenString = token.TokenString;
				}
				else
					tokenString = first.TokenString;


				return GetApiKey(tokenString);
			}
			return null;
		}

		[HttpGet("[action]")]
		public string Posts()
		{
			var list = _db.Posts.Where(x => x.Status == Status.Published).ToList();
			return JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
		}

		[HttpGet("[action]")]
		public string MyUser(string apiKey)
		{
			if (!AuthenticateKey(apiKey, out var user))
				return GetErrorResponse("401", "Invalid API Key");

			if (user != null)
				return JsonSerializer.Serialize(user, new JsonSerializerOptions { WriteIndented = true });
			return GetErrorResponse("404", "User not found");
		}

		[HttpGet("[action]")]
		public string Users(string apiKey)
		{
			if (!AuthenticateKey(apiKey, out var user))
				return GetErrorResponse("401", "Invalid API Key");

			if (user == null)
				return GetErrorResponse("404", "Keyholder not found.");

			if (user.Role == Role.Member || user.Role == Role.MemberNoAuth)
				return GetErrorResponse("401", "You are not authorized for this action.");

			return JsonSerializer.Serialize(_db.Users.Select(x => x.Id.ToString()).ToList(), new JsonSerializerOptions { WriteIndented = true });
		}

		[HttpGet("[action]")]
		public string GetUser(string apiKey, string userId)
		{
			if (!AuthenticateKey(apiKey, out var user))
				return GetErrorResponse("401", "Invalid API Key.");

			if (user == null)
				return GetErrorResponse("404", "Keyholder not found.");

			if (Guid.TryParse(userId, out var guid))
			{
				if (!user.Id.Equals(guid))
					if (user.Role == Role.Member || user.Role == Role.MemberNoAuth)
						return GetErrorResponse("401", "You are not authorized for this action.");

				var requestedUser = _db.Users.FirstOrDefault(x => x.Id.Equals(guid));
				if (requestedUser == null)
					return GetErrorResponse("404", "The requested user couldn't be found.");

				return JsonSerializer.Serialize(requestedUser, new JsonSerializerOptions { WriteIndented = true });
			}
			return GetErrorResponse("400", "Invalid User ID.");
		}

		[HttpGet("[action]")]
		public string Post(string apiKey, string postId)
		{
			if (!AuthenticateKey(apiKey, out var user))
				return GetErrorResponse("401", "Invalid API Key.");

			if (user == null)
				return GetErrorResponse("404", "Keyholder not found.");

			if (Guid.TryParse(postId, out var guid))
			{
				var post = _db.Posts.FirstOrDefault(x => x.Id.Equals(guid));
				if (post == null)
					return GetErrorResponse("404", "Post not found.");

				if (post.Status != Status.Published)
				{
					if (user.Role == Role.Member)
						if (!post.UserId.Equals(user.Id))
							return GetErrorResponse("401", "You are not authorized to view this post.");
				}

				return JsonSerializer.Serialize(post, new JsonSerializerOptions { WriteIndented = true });
			}
			return GetErrorResponse("400", "Invalid Post ID.");
		}

		[HttpGet("[action]")]
		public string Comments(string apiKey, string postId)
		{
			if (!AuthenticateKey(apiKey, out var user))
				return GetErrorResponse("401", "Invalid API Key.");

			if (user == null)
				return GetErrorResponse("404", "Keyholder not found.");

			if (Guid.TryParse(postId, out var guid))
			{
				var post = _db.Posts.FirstOrDefault(x => x.Id.Equals(guid));
				if (post == null)
					return GetErrorResponse("404", "Post not found.");

				if (post.Status != Status.Published)
				{
					if (user.Role == Role.Member)
						if (!post.UserId.Equals(user.Id))
							return GetErrorResponse("401", "You are not authorized to view this post or its comments.");
				}

				if (post.Comments.Any())
					return JsonSerializer.Serialize(post.Comments.ToList(), new JsonSerializerOptions { WriteIndented = true });
				else
					return GetErrorResponse("400", "This post has no comments.");
			}
			return GetErrorResponse("400", "Invalid Post ID.");
		}
	}
}