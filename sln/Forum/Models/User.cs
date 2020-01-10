using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Forum.Business;
using Forum.Business.Helpers;

namespace Forum.Models
{
	public class User
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		[Required]
		public string Username { get; set; }
		[Required]
		public string Password { get; set; }
		[Required]
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
		public virtual ICollection<Post> Posts { get; set; }
		public virtual ICollection<Token> Tokens { get; set; }
		public virtual ICollection<Comment> Comments { get; set; }
	}
}
