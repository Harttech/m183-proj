using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using Forum.Business;
using Forum.Business.Helpers;
using Forum.Controllers;

namespace Forum.Models
{
	[Serializable]
	public class User
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		[Required]
		public string Username { get; set; }
		[Required]
		[JsonIgnore]
		public string Password { get; set; }
		[Required]
		[JsonIgnore]
		public string Salt { get; set; } = CryptoHelper.GenerateRandomSalt();
		[Required]
		public string PhoneNumber { get; set; }
		[Required]
		public bool AccessBlocked { get; set; }
		public DateTime? AccessBlockedUntilUtc { get; set; }
		[Required]
		[EmailAddress]
		public string Email { get; set; }
		[Required]
		public Role Role { get; set; }
		[JsonIgnore]
		public virtual ICollection<Post> Posts { get; set; }
		[JsonIgnore]
		public virtual ICollection<Token> Tokens { get; set; }
		[JsonIgnore]
		public virtual ICollection<Comment> Comments { get; set; }
		
		public string GetCurrentApiToken()
		{
			var token = DataBase.Instance.Tokens.FirstOrDefault(x => x.TokenKind == TokenKind.ApiAuthToken && DateTime.UtcNow < x.ExpiryUtc && x.UserId.Equals(Id))?.TokenString;
			if (token.NullOrWhiteSpace())
				return null;

			return ApiController.GetToken(this);
		}
	}
}
