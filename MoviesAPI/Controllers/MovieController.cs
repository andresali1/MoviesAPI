using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using MoviesAPI.Interfaces;

namespace MoviesAPI.Controllers
{
    [ApiController]
    [Route("api/movie")]
    public class MovieController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IStoreFile _storeFile;
        private readonly string _container = "movies";

        public MovieController(ApplicationDbContext context,
            IMapper mapper,
            IStoreFile storeFile)
        {
            _context = context;
            _mapper = mapper;
            _storeFile = storeFile;
        }

        /// <summary>
        /// Method to get all the movies in DB
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "getMovies")]
        public async Task<ActionResult<List<MovieDTO>>> Get()
        {
            var movies = await _context.Movie.ToListAsync();
            return _mapper.Map<List<MovieDTO>>(movies);
        }

        /// <summary>
        /// Method to get a Movie by its Id
        /// </summary>
        /// <param name="id">Id of the Movie</param>
        /// <returns></returns>
        [HttpGet("{id:int}", Name = "getMovie")]
        public async Task<ActionResult<MovieDTO>> Get(int id)
        {
            var entity = await _context.Movie.FirstOrDefaultAsync(m => m.Id == id);

            if (entity == null)
            {
                return NotFound();
            }

            return _mapper.Map<MovieDTO>(entity);
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
            var movieDB = await _context.Movie.FirstOrDefaultAsync(m => m.Id == id);

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

            await _context.SaveChangesAsync();
            return NoContent();
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
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var entityDB = await _context.Movie.FirstOrDefaultAsync(m => m.Id == id);

            if (entityDB == null)
            {
                return NotFound();
            }

            var entityDTO = _mapper.Map<MoviePatchDTO>(entityDB);

            patchDocument.ApplyTo(entityDTO, ModelState);

            var isValid = TryValidateModel(entityDTO);

            if (!isValid)
            {
                return BadRequest(ModelState);
            }

            _mapper.Map(entityDTO, entityDB);

            await _context.SaveChangesAsync();

            return NoContent();
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
