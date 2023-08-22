using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.DTOs
{
    public class GenreCreationDTO
    {
        [Required]
        [StringLength(40)]
        public string G_Name { get; set; }
    }
}
