using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Entities
{
    public class Movie
    {
        public int Id { get; set; }

        [Required]
        [StringLength(300)]
        public string Title { get; set; }

        public bool JustReleased { get; set; }

        public DateTime RealeaseDate { get; set; }

        public string Poster { get; set; }
    }
}
