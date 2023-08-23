using MoviesAPI.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Entities
{
    public class Genre : IId
    {
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        public string G_Name { get; set; }

        public List<MovieGenre> MovieGenre { get; set; }
    }
}
