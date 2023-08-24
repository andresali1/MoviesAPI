using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;

namespace MoviesAPI.Controllers
{
    [ApiController]
    [Route("api/cinema")]
    public class CinemaController : CustomBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CinemaController(ApplicationDbContext context,
            IMapper mapper)
            : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Method to get all the Cinemas in DB
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "getCinemas")]
        public async Task<ActionResult<List<CinemaDTO>>> Get()
        {
            return await Get<Cinema, CinemaDTO>();
        }

        /// <summary>
        /// Method to get a Cinema by its id
        /// </summary>
        /// <param name="id">Id of the Cinema</param>
        /// <returns></returns>
        [HttpGet("{id:int}", Name = "getCinema")]
        public async Task<ActionResult<CinemaDTO>> Get(int id)
        {
            return await Get<Cinema, CinemaDTO>(id);
        }

        /// <summary>
        /// Method to create a new Cinema in DB
        /// </summary>
        /// <param name="cinemaCreationDTO">Object with Cinema Data to create</param>
        /// <returns></returns>
        [HttpPost(Name = "createCinema")]
        public async Task<ActionResult> Post([FromBody] CinemaCreationDTO cinemaCreationDTO)
        {
            return await Post<CinemaCreationDTO, Cinema, CinemaDTO>(cinemaCreationDTO, "getCinema");
        }

        /// <summary>
        /// Method to update the entire data of a Cinema in BD
        /// </summary>
        /// <param name="id">Id of the Cinema</param>
        /// <param name="cinemaCreationDTO">Object with Cinema Data to create</param>
        /// <returns></returns>
        [HttpPut("{id:int}", Name = "putCinema")]
        public async Task<ActionResult> Put(int id, [FromBody] CinemaCreationDTO cinemaCreationDTO)
        {
            return await Put<CinemaCreationDTO, Cinema>(id, cinemaCreationDTO);
        }

        /// <summary>
        /// Method to delete a Cinema from DB
        /// </summary>
        /// <param name="id">Id of the Cinema</param>
        /// <returns></returns>
        [HttpDelete("{id:int}", Name = "deleteCinema")]
        public async Task<ActionResult> Delete(int id)
        {
            return await Delete<Cinema>(id);
        }
    }
}
