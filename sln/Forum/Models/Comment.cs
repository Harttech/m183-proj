using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Forum.Models
{
	[Serializable]
	public class Comment
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid PostId { get; set; }
		public Guid UserId { get; set; }
		[Required]
		public string Content { get; set; }
		[JsonIgnore]
		public virtual Post Post { get; set; }
		[JsonIgnore]
		public virtual User User { get; set; }
		public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
	}
}
