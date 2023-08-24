using MoviesAPI.Interfaces;
using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Entities
{
    public class Cinema : IId
    {
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        public string C_Name { get; set; }

        public Point Location { get; set; }

        public List<MovieCinema> MovieCinema { get; set; }
    }
}
