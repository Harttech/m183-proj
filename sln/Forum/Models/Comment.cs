using System;
using System.ComponentModel.DataAnnotations;

namespace Forum.Models
{
	public class Comment
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid PostId { get; set; }
		public Guid UserId { get; set; }
		[Required]
		public string Content { get; set; }
		public virtual Post Post { get; set; }
		public virtual User User { get; set; }
	}
}
