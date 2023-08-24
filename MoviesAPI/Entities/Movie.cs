using MoviesAPI.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Entities
{
    public class Movie : IId
    {
        public int Id { get; set; }

        [Required]
        [StringLength(300)]
        public string Title { get; set; }

        public bool JustReleased { get; set; }

        public DateTime RealeaseDate { get; set; }

        public string Poster { get; set; }

        public List<MovieActor> MovieActor { get; set; }

        public List<MovieGenre> MovieGenre { get; set; }

        public List<MovieCinema> MovieCinema { get; set; }
    }
}
