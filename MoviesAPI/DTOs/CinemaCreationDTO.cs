using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.DTOs
{
    public class CinemaCreationDTO
    {
        [Required]
        [StringLength(120)]
        public string C_Name { get; set; }

        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Range(-180, 180)]
        public double Longitude { get; set; }
    }
}
