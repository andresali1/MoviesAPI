using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using MoviesAPI.Helpers;
using MoviesAPI.Interfaces;
using System.Linq.Dynamic.Core;

namespace MoviesAPI.Controllers
{
    [ApiController]
    [Route("api/movie")]
    public class MovieController : CustomBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IStoreFile _storeFile;
        private readonly ILogger _logger;
        private readonly string _container = "movies";

        public MovieController(ApplicationDbContext context,
            IMapper mapper,
            IStoreFile storeFile,
            ILogger<MovieController> logger)
            : base (context, mapper)
        {
            _context = context;
            _mapper = mapper;
            _storeFile = storeFile;
            _logger = logger;
        }

        /// <summary>
        /// Method to get the newest movies
        /// </summary>
        /// <returns></returns>
        [HttpGet("latest", Name = "getLatest")]
        public async Task<ActionResult<MovieIndexDTO>> GetLatest()
        {
            var top = 3;
            var today = DateTime.Today;

            var comingReleases = await _context.Movie
                .Where(m => m.RealeaseDate > today)
                .OrderBy(m => m.RealeaseDate)
                .Take(top)
                .ToListAsync();

            var recentReleases = await _context.Movie
                .Where(m => m.JustReleased)
                .Take(top)
                .ToListAsync();

            var result = new MovieIndexDTO();
            result.ComingReleases = _mapper.Map<List<MovieDTO>>(comingReleases);
            result.InTheaters = _mapper.Map<List<MovieDTO>>(recentReleases);

            return result;
        }

        /// <summary>
        /// Method to get movies by given filters in the query string
        /// </summary>
        /// <param name="movieFilterDTO">Object with the filters</param>
        /// <returns></returns>
        [HttpGet("filter", Name = "getFilter")]
        public async Task<ActionResult<List<MovieDTO>>> Filter([FromQuery] MovieFilterDTO movieFilterDTO)
        {
            var moviesQueryable = _context.Movie.AsQueryable();

            if (!string.IsNullOrEmpty(movieFilterDTO.Title))
            {
                moviesQueryable = moviesQueryable.Where(m => m.Title.Contains(movieFilterDTO.Title));
            }

            if (movieFilterDTO.JustReleased)
            {
                moviesQueryable = moviesQueryable.Where(m => m.JustReleased);
            }

            if (movieFilterDTO.ComingRelease)
            {
                var today = DateTime.Today;
                moviesQueryable = moviesQueryable.Where(m => m.RealeaseDate > today);
            }

            if(movieFilterDTO.GenreId != 0)
            {
                moviesQueryable = moviesQueryable.Where(m => m.MovieGenre.Select(mg => mg.GenreId)
                    .Contains(movieFilterDTO.GenreId));
            }

            if (!string.IsNullOrEmpty(movieFilterDTO.OrderField))
            {
                var orderType = movieFilterDTO.AscendingOrder ? "ascending" : "descending";

                try
                {
                    moviesQueryable = moviesQueryable.OrderBy($"{movieFilterDTO.OrderField} {orderType}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                }
            }

            await HttpContext.InsertPaginationParams(moviesQueryable, movieFilterDTO.RecordsPerPage);

            var movies = await moviesQueryable.Page(movieFilterDTO.Pagination).ToListAsync();

            return _mapper.Map<List<MovieDTO>>(movies);
        }

        /// <summary>
        /// Method to get a Movie by its Id
        /// </summary>
        /// <param name="id">Id of the Movie</param>
        /// <returns></returns>
        [HttpGet("{id:int}", Name = "getMovie")]
        public async Task<ActionResult<MovieDetailDTO>> Get(int id)
        {
            var entity = await _context.Movie
                .Include(m => m.MovieActor).ThenInclude(m => m.Actor)
                .Include(m => m.MovieGenre).ThenInclude(m => m.Genre)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (entity == null)
            {
                return NotFound();
            }

            entity.MovieActor = entity.MovieActor.OrderBy(m => m.Order_Num).ToList();

            return _mapper.Map<MovieDetailDTO>(entity);
        }

        /// <summary>
        /// Method to create a new movie in DB
        /// </summary>
        /// <param name="movieCreationDTO">Object with movie's information to save</param>
        /// <returns></returns>
        [HttpPost(Name = "createMovie")]
        public async Task<ActionResult> Post([FromForm] MovieCreationDTO movieCreationDTO)
        {
            var entity = _mapper.Map<Movie>(movieCreationDTO);

            if (movieCreationDTO.Poster != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await movieCreationDTO.Poster.CopyToAsync(memoryStream);
                    var content = memoryStream.ToArray();
                    var extension = Path.GetExtension(movieCreationDTO.Poster.FileName);
                    entity.Poster = await _storeFile.SaveFile(content, extension, _container, movieCreationDTO.Poster.ContentType);
                }
            }

            _context.Add(entity);
            await _context.SaveChangesAsync();
            var movieDto = _mapper.Map<MovieDTO>(entity);
            return new CreatedAtRouteResult("getMovie", new { id = entity.Id }, movieDto);
        }

        /// <summary>
        /// Method to update a movie
        /// </summary>
        /// <param name="id">Id of the movie</param>
        /// <param name="movieCreationDTO">Object with movie's data to update</param>
        /// <returns></returns>
        [HttpPut("{id:int}", Name = "putMovie")]
        public async Task<ActionResult> Put(int id, [FromForm] MovieCreationDTO movieCreationDTO)
        {
            var movieDB = await _context.Movie
                .Include(m => m.MovieActor)
                .Include(m => m.MovieGenre)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movieDB == null)
            {
                return NotFound();
            }

            movieDB = _mapper.Map(movieCreationDTO, movieDB);

            if (movieCreationDTO.Poster != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await movieCreationDTO.Poster.CopyToAsync(memoryStream);
                    var content = memoryStream.ToArray();
                    var extension = Path.GetExtension(movieCreationDTO.Poster.FileName);
                    movieDB.Poster = await _storeFile.EditFile(content, extension, _container, movieDB.Poster, movieCreationDTO.Poster.ContentType);
                }
            }

            AssignActorsOrder(movieDB);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Method to assing the order of the actors of the movie
        /// </summary>
        /// <param name="movie">Movie with the actors data</param>
        private void AssignActorsOrder(Movie movie)
        {
            if(movie.MovieActor != null)
            {
                for(int i = 0; i < movie.MovieActor.Count; i++)
                {
                    movie.MovieActor[i].Order_Num = i;
                }
            }
        }

        /// <summary>
        /// Method to patch a movie
        /// </summary>
        /// <param name="id">Id of the movie</param>
        /// <param name="patchDocument">Object with the information of the field to update</param>
        /// <returns></returns>
        [HttpPatch("{id:int}", Name = "patchMovie")]
        public async Task<ActionResult> Patch(int id, [FromBody] JsonPatchDocument<MoviePatchDTO> patchDocument)
        {
            return await Patch<Movie, MoviePatchDTO>(id, patchDocument);
        }

        /// <summary>
        /// Method to delete a Movie from DB
        /// </summary>
        /// <param name="id">Id of the Movie</param>
        /// <returns></returns>
        [HttpDelete("{id:int}", Name = "deleteMovie")]
        public async Task<ActionResult> Delete(int id)
        {
            var movieDB = await _context.Movie.FirstOrDefaultAsync(m => m.Id == id);

            if (movieDB == null)
            {
                return NotFound();
            }

            if (movieDB.Poster != null)
            {
                await _storeFile.DeleteFile(movieDB.Poster, _container);
            }

            _context.Remove(movieDB);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
