using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Entities;
using System.Security.Claims;

namespace MoviesAPI
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //To set compound key
            modelBuilder.Entity<MovieGenre>()
                .HasKey(x => new { x.GenreId, x.MovieId });

            //To set compound key
            modelBuilder.Entity<MovieActor>()
                .HasKey(x => new { x.ActorId, x.MovieId });

            //To set compound key
            modelBuilder.Entity<MovieCinema>()
                .HasKey(x => new { x.CinemaId, x.MovieId });

            SeedData(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Method to manually seed the Db Data
        /// </summary>
        /// <param name="modelBuilder"></param>
        private void SeedData(ModelBuilder modelBuilder)
        {
            var adminRoleId = "ecd8fff3-5ce4-48a0-b8a9-ab036c22bc71";
            var userAdminId = "92cd1723-340a-447c-b5fb-3b6d5cc93212";

            var adminRole = new IdentityRole()
            {
                Id = adminRoleId,
                Name = "Admin",
                NormalizedName = "Admin"
            };

            var passworHasher = new PasswordHasher<IdentityUser>();

            var username = "admin@mail.com";

            var adminUser = new IdentityUser()
            {
                Id = userAdminId,
                UserName = username,
                NormalizedUserName = username,
                Email = username,
                NormalizedEmail = username,
                PasswordHash = passworHasher.HashPassword(null, "Admin123.")
            };

            //modelBuilder.Entity<IdentityUser>()
            //    .HasData(adminUser);

            //modelBuilder.Entity<IdentityRole>()
            //    .HasData(adminRole);

            //modelBuilder.Entity<IdentityUserClaim<string>>()
            //    .HasData(new IdentityUserClaim<string>()
            //    {
            //        Id = 1,
            //        ClaimType = ClaimTypes.Role,
            //        UserId = userAdminId,
            //        ClaimValue = "Admin"
            //    });
        }

        public DbSet<Genre> Genre { get; set; }
        public DbSet<Actor> Actor { get; set; }
        public DbSet<Movie> Movie { get; set; }
        public DbSet<Cinema> Cinema { get; set; }
        public DbSet<MovieGenre> MovieGenre { get; set; }
        public DbSet<MovieActor> MovieActor { get; set; }
        public DbSet<MovieCinema> MovieCinema { get; set; }
        public DbSet<Review> Review { get; set; }
    }
}
