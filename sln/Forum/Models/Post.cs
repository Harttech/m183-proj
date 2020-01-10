using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Forum.Business;

namespace Forum.Models
{
	public class Post
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid UserId { get; set; }
		public virtual User User { get; set; }
		[Required]
		public string Title { get; set; }
		[Required]
		public string Content { get; set; }
		public virtual ICollection<Comment> Comments { get; set; }
		public Status Status { get; set; } = Status.Hidden;
		public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
	}
}
