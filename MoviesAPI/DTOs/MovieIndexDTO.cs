namespace MoviesAPI.DTOs
{
    public class MovieIndexDTO
    {
        public List<MovieDTO> ComingReleases { get; set; }
        public List<MovieDTO> InTheaters { get; set; }
    }
}
