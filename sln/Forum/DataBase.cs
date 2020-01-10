using Forum.Models;
using Microsoft.EntityFrameworkCore;

namespace Forum
{
	public class DataBase : DbContext
	{
		public static DataBase Instance => new DataBase();
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseLazyLoadingProxies().UseSqlite("Filename=MhForum.db");
		}

		public DbSet<User> Users { get; set; }
		public DbSet<Post> Posts { get; set; }
		public DbSet<Comment> Comments { get; set; }
		public DbSet<Token> Tokens { get; set; }
	}
}
