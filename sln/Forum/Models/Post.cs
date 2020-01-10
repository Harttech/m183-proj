using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Forum.Business;

namespace Forum.Models
{
	[Serializable]
	public class Post
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid UserId { get; set; }
		[JsonIgnore]
		public virtual User User { get; set; }
		[Required]
		public string Title { get; set; }
		[Required]
		public string Content { get; set; }
		[JsonIgnore]
		public virtual ICollection<Comment> Comments { get; set; }
		public Status Status { get; set; } = Status.Hidden;
		public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
	}
}
