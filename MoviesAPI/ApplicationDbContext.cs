using Microsoft.EntityFrameworkCore;
using MoviesAPI.Entities;

namespace MoviesAPI
{
    public class ApplicationDbContext : DbContext
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

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Genre> Genre { get; set; }
        public DbSet<Actor> Actor { get; set; }
        public DbSet<Movie> Movie { get; set; }
        public DbSet<MovieGenre> MovieGenre { get; set; }
        public DbSet<MovieActor> MovieActor { get; set; }
    }
}
