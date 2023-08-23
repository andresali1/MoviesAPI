namespace MoviesAPI.DTOs
{
    public class MovieDetailDTO : MovieDTO
    {
        public List<GenreDTO> Genres { get; set; }
        public List<MovieActorDetailDTO> Actors { get; set; }
    }
}
