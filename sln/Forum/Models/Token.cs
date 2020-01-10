using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Forum.Business;

namespace Forum.Models
{
	[Serializable]
	public class Token
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid UserId { get; set; }
		[JsonIgnore]
		public virtual User User { get; set; }
		[Required]
		public string TokenString { get; set; }
		[Required]
		public DateTime ExpiryUtc { get; set; }
		public TokenKind TokenKind { get; set; } = TokenKind.LoginAuthToken;
	}
}
