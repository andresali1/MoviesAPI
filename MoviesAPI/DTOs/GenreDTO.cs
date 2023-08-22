using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.DTOs
{
    public class GenreDTO
    {
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        [Display(Name = "Name")]
        public string G_Name { get; set; }
    }
}
