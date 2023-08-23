using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.DTOs
{
    public class ActorDTO
    {
        public int Id { get; set; }

        public string A_Name { get; set; }

        public DateTime BirthDate { get; set; }

        public string Photo { get; set; }
    }
}
