using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.DTOs
{
    public class MovieDTO
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public bool JustReleased { get; set; }

        public DateTime RealeaseDate { get; set; }

        public string Poster { get; set; }
    }
}
