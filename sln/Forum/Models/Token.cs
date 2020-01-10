using System;
using System.ComponentModel.DataAnnotations;

namespace Forum.Models
{
	public class Token
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid UserId { get; set; }
		public virtual User User { get; set; }
		[Required]
		public string TokenString { get; set; }
		[Required]
		public DateTime ExpiryUtc { get; set; }
	}
}
