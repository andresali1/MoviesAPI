using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.DTOs
{
    public class GenreCreationDTO
    {
        [Required]
        [StringLength(40)]
        [Display(Name = "Name")]
        public string G_Name { get; set; }
    }
}
