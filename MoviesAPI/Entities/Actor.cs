using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Entities
{
    public class Actor
    {
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        public string A_Name { get; set; }

        public DateTime BirthDate { get; set; }

        public string Photo { get; set; }

        public List<MovieActor> MovieActor { get; set; }
    }
}
