using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.DTOs
{
    public class MoviePatchDTO
    {
        [Required]
        [StringLength(300)]
        public string Title { get; set; }

        public bool JustReleased { get; set; }

        public DateTime RealeaseDate { get; set; }
    }
}
