using Microsoft.EntityFrameworkCore;
using MeawMarket.Models;
using MeawMarket.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Design;

namespace MeawMarket.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }
        public DbSet<Cat> Cats { get; set; }
        public DbSet<SoldCat> SoldCats { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cat>()
                .Property(c => c.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Cat>()
                .Property(c => c.Status)
                .HasDefaultValue("Available");

          
        }
        public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
        {
            public AppDbContext CreateDbContext(string[] args)
            {
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseSqlServer("Server=LAPTOP-ACCQFJJ2\\SQLEXPRESS;Database=MeawMarket;Trusted_Connection=True;MultipleActiveResultSets=true");

                return new AppDbContext(optionsBuilder.Options);
            }
        }

    }

}


