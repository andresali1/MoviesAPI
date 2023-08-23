using AutoMapper;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;

namespace MoviesAPI.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            //Genre
            CreateMap<Genre, GenreDTO>().ReverseMap();
            CreateMap<GenreCreationDTO, Genre>();

            //Actor
            CreateMap<Actor, ActorDTO>().ReverseMap();
            CreateMap<ActorCreationDTO, Actor>()
                .ForMember(x => x.Photo, options => options.Ignore());

            CreateMap<ActorPatchDTO, Actor>().ReverseMap();

            //Movie
            CreateMap<Movie, MovieDTO>().ReverseMap();
            CreateMap<MovieCreationDTO, Movie>()
                .ForMember(x => x.Poster, options => options.Ignore())
                .ForMember(x => x.MovieGenre, options => options.MapFrom(MapMovieGenre))
                .ForMember(x => x.MovieActor, options => options.MapFrom(MapMovieActor));

            CreateMap<Movie, MovieDetailDTO>()
                .ForMember(x => x.Genres, options => options.MapFrom(MapMovieGenre))
                .ForMember(x => x.Actors, options => options.MapFrom(MapMovieActor));

            CreateMap<MoviePatchDTO, Movie>().ReverseMap();
        }

        /// <summary>
        /// Method to Custom mapping MovieGenre into Movie
        /// </summary>
        /// <param name="movieCreationDTO">Object with sent data</param>
        /// <param name="movie">Object to receive the data</param>
        /// <returns></returns>
        private List<MovieGenre> MapMovieGenre(MovieCreationDTO movieCreationDTO, Movie movie)
        {
            var result = new List<MovieGenre>();

            if(movieCreationDTO.GenreIds == null) { return result; }

            foreach(var id in movieCreationDTO.GenreIds)
            {
                result.Add(new MovieGenre() { GenreId = id });
            }

            return result;
        }

        /// <summary>
        /// Method to Custom mapping MovieActor into Movie
        /// </summary>
        /// <param name="movieCreationDTO">Object with sent data</param>
        /// <param name="movie">Object to receive the data</param>
        /// <returns></returns>
        private List<MovieActor> MapMovieActor(MovieCreationDTO movieCreationDTO, Movie movie)
        {
            var result = new List<MovieActor>();

            if (movieCreationDTO.Actors == null) { return result; }

            foreach (var actor in movieCreationDTO.Actors)
            {
                result.Add(new MovieActor() { ActorId = actor.ActorId, Character = actor.Character });
            }

            return result;
        }

        /// <summary>
        /// Method to Custom mapping Movie Genre into MovieDetailDTO
        /// </summary>
        /// <param name="movie">Object with sent data</param>
        /// <param name="movieDetailDTO">Object to receive the data</param>
        /// <returns></returns>
        private List<GenreDTO> MapMovieGenre(Movie movie, MovieDetailDTO movieDetailDTO)
        {
            var result = new List<GenreDTO>();

            if(movie.MovieGenre == null) { return result; }

            foreach(var movieGenre in movie.MovieGenre)
            {
                result.Add(new GenreDTO() { Id = movieGenre.GenreId, G_Name = movieGenre.Genre.G_Name });
            }

            return result;
        }

        /// <summary>
        /// Method to custom mapping Movie Actor into MovieDetailDTO
        /// </summary>
        /// <param name="movie">Object with sent data</param>
        /// <param name="movieDetailDTO">Object to receive the data</param>
        /// <returns></returns>
        private List<MovieActorDetailDTO> MapMovieActor(Movie movie, MovieDetailDTO movieDetailDTO)
        {
            var result = new List<MovieActorDetailDTO>();

            if (movie.MovieActor == null) { return result; }

            foreach (var movieActor in movie.MovieActor)
            {
                result.Add(new MovieActorDetailDTO() { ActorId = movieActor.ActorId, Character = movieActor.Character, RealName = movieActor.Actor.A_Name });
            }

            return result;
        }
    }
}
