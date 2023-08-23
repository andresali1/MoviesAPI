using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using System.Runtime.CompilerServices;

namespace MoviesAPI.Controllers
{
    [ApiController]
    [Route("api/genre")]
    public class GenreController : CustomBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GenreController(ApplicationDbContext context,
            IMapper mapper)
            :base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Method to get all the genres in DB
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "getGenres")]
        public async Task<ActionResult<List<GenreDTO>>> Get()
        {
            return await Get<Genre, GenreDTO>();
        }

        /// <summary>
        /// Method to get a Genre by it Id
        /// </summary>
        /// <param name="id">Id of the Genre</param>
        /// <returns></returns>
        [HttpGet("{id:int}", Name = "getGenre")]
        public async Task<ActionResult<GenreDTO>> Get(int id)
        {
            return await Get<Genre, GenreDTO>(id);
        }

        /// <summary>
        /// Method to create a new Genre in DB
        /// </summary>
        /// <param name="genreCreationDTO">Object with new genre's data</param>
        /// <returns></returns>
        [HttpPost(Name = "createGenre")]
        public async Task<ActionResult> Post([FromBody] GenreCreationDTO genreCreationDTO)
        {
            return await Post<GenreCreationDTO, Genre, GenreDTO>(genreCreationDTO, "getGenre");
        }

        /// <summary>
        /// Method to update a Genre in DB
        /// </summary>
        /// <param name="id">Id of the Genre</param>
        /// <param name="genreCreationDTO">Object with the Genre data to update</param>
        /// <returns></returns>
        [HttpPut("{id:int}", Name = "putGenre")]
        public async Task<ActionResult> Put(int id, [FromBody] GenreCreationDTO genreCreationDTO)
        {
            var exists = await _context.Genre.AnyAsync(g => g.Id == id);

            if (!exists)
            {
                return NotFound();
            }

            var entity = _mapper.Map<Genre>(genreCreationDTO);
            entity.Id = id;
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Method to delete an existing Genre from DB
        /// </summary>
        /// <param name="id">Id of the Genre</param>
        /// <returns></returns>
        [HttpDelete("{id:int}", Name = "deleteGenre")]
        public async Task<ActionResult> Delete(int id)
        {
            var exists = await _context.Genre.AnyAsync(g => g.Id == id);

            if (!exists)
            {
                return NotFound();
            }

            _context.Remove(new Genre() { Id = id });
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
