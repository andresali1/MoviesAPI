using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using NetTopologySuite.Geometries;

namespace MoviesAPI.Controllers
{
    [ApiController]
    [Route("api/cinema")]
    public class CinemaController : CustomBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly GeometryFactory _geometryFactory;

        public CinemaController(ApplicationDbContext context,
            IMapper mapper,
            GeometryFactory geometryFactory)
            : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
            _geometryFactory = geometryFactory;
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
        /// Method to get the cinemas acording to distance from filters
        /// </summary>
        /// <param name="filter">Object with distance filters</param>
        /// <returns></returns>
        [HttpGet("nearby", Name = "getNearCinemas")]
        public async Task<ActionResult<List<NearCinemaDTO>>> Get([FromQuery] NearCinemaFilterDTO filter)
        {
            var userLocation = _geometryFactory.CreatePoint(new Coordinate(filter.Longitude, filter.Latitude));

            var cinemas = await _context.Cinema
                .OrderBy(c => c.Location.Distance(userLocation))
                .Where(c => c.Location.IsWithinDistance(userLocation, filter.DistanceInKm * 1000))
                .Select(c => new NearCinemaDTO
                {
                    Id = c.Id,
                    C_Name = c.C_Name,
                    Latitude = c.Location.Y,
                    Longitude = c.Location.X,
                    DistanceInMt = Math.Round(c.Location.Distance(userLocation))
                }).ToListAsync();

            return cinemas;
        }

        /// <summary>
        /// Method to create a new Cinema in DB
        /// </summary>
        /// <param name="cinemaCreationDTO">Object with Cinema Data to create</param>
        /// <returns></returns>
        [HttpPost(Name = "createCinema")]
        public async Task<ActionResult> Post([FromBody] CinemaCreationDTO cinemaCreationDTO)
        {
            var entity = _mapper.Map<Cinema>(cinemaCreationDTO);
            entity.Location = new Point(cinemaCreationDTO.Longitude, cinemaCreationDTO.Latitude) { SRID = 4326 };
            _context.Add(entity);
            await _context.SaveChangesAsync();
            var readDTO = _mapper.Map<CinemaDTO>(entity);

            return new CreatedAtRouteResult("getCinema", new { id = entity.Id }, readDTO);
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
            var exists = await _context.Cinema.AnyAsync(c => c.Id == id);

            if (!exists)
            {
                return NotFound();
            }

            var entity = _mapper.Map<Cinema>(cinemaCreationDTO);
            entity.Location = new Point(cinemaCreationDTO.Longitude, cinemaCreationDTO.Latitude) { SRID = 4326 };
            entity.Id = id;
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
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
