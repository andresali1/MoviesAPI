using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using MoviesAPI.Helpers;
using MoviesAPI.Interfaces;

namespace MoviesAPI.Controllers
{
    [ApiController]
    [Route("api/actor")]
    public class ActorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IStoreFile _storeFile;
        private readonly string _container = "actors";

        public ActorController(ApplicationDbContext context,
            IMapper mapper,
            IStoreFile storeFile)
        {
            _context = context;
            _mapper = mapper;
            _storeFile = storeFile;
        }

        /// <summary>
        /// Method to get all the actors in DB
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "getActors")]
        public async Task<ActionResult<List<ActorDTO>>> Get([FromQuery] PaginationDTO paginationDTO)
        {
            var queryable = _context.Actor.AsQueryable();
            await HttpContext.InsertPaginationParams(queryable, paginationDTO.RecordsPerPage);
            var entities = await queryable.Page(paginationDTO).ToListAsync();
            var dtos = _mapper.Map<List<ActorDTO>>(entities);
            return dtos;
        }

        /// <summary>
        /// Method to get an Actor by its Id
        /// </summary>
        /// <param name="id">Id of the Actor</param>
        /// <returns></returns>
        [HttpGet("{id:int}", Name = "getActor")]
        public async Task<ActionResult<ActorDTO>> Get(int id)
        {
            var entity = await _context.Actor.FirstOrDefaultAsync(a => a.Id == id);

            if (entity == null)
            {
                return NotFound();
            }

            var actorDto = _mapper.Map<ActorDTO>(entity);
            return actorDto;
        }

        /// <summary>
        /// Method to create a new Actor in DB
        /// </summary>
        /// <param name="actorCreationDTO">Object with Actor's data to save</param>
        /// <returns></returns>
        [HttpPost(Name = "createActor")]
        public async Task<ActionResult> Post([FromForm] ActorCreationDTO actorCreationDTO)
        {
            var entity = _mapper.Map<Actor>(actorCreationDTO);

            if(actorCreationDTO.Photo != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await actorCreationDTO.Photo.CopyToAsync(memoryStream);
                    var content = memoryStream.ToArray();
                    var extension = Path.GetExtension(actorCreationDTO.Photo.FileName);
                    entity.Photo = await _storeFile.SaveFile(content, extension, _container, actorCreationDTO.Photo.ContentType);
                }
            }

            _context.Add(entity);
            await _context.SaveChangesAsync();
            var actorDto = _mapper.Map<ActorDTO>(entity);
            return new CreatedAtRouteResult("getActor", new { id = entity.Id }, actorDto);
        }

        /// <summary>
        /// Method to update an Actor in DB
        /// </summary>
        /// <param name="id">Id of the actor</param>
        /// <param name="actorCreationDTO">Object with Actor's dat ato update</param>
        /// <returns></returns>
        [HttpPut("{id:int}", Name = "putActor")]
        public async Task<ActionResult> Put(int id, [FromForm] ActorCreationDTO actorCreationDTO)
        {
            var actorDB = await _context.Actor.FirstOrDefaultAsync(a => a.Id == id);

            if (actorDB == null)
            {
                return NotFound();
            }

            actorDB = _mapper.Map(actorCreationDTO, actorDB);

            if (actorCreationDTO.Photo != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await actorCreationDTO.Photo.CopyToAsync(memoryStream);
                    var content = memoryStream.ToArray();
                    var extension = Path.GetExtension(actorCreationDTO.Photo.FileName);
                    actorDB.Photo = await _storeFile.EditFile(content, extension, _container, actorDB.Photo, actorCreationDTO.Photo.ContentType);
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id:int}", Name = "patchActor")]
        public async Task<ActionResult> Patch(int id, [FromBody] JsonPatchDocument<ActorPatchDTO> patchDocument)
        {
            if(patchDocument == null)
            {
                return BadRequest();
            }

            var entityDB = await _context.Actor.FirstOrDefaultAsync(a => a.Id == id);

            if(entityDB == null)
            {
                return NotFound();
            }

            var entityDTO = _mapper.Map<ActorPatchDTO>(entityDB);

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
        /// Method to delete an Actor from Db
        /// </summary>
        /// <param name="id">Id of the actor</param>
        /// <returns></returns>
        [HttpDelete("{id:int}", Name = "deleteActor")]
        public async Task<ActionResult> Delete(int id)
        {
            var actorDB = await _context.Actor.FirstOrDefaultAsync(a => a.Id == id);

            if (actorDB == null)
            {
                return NotFound();
            }

            if(actorDB.Photo != null)
            {
                await _storeFile.DeleteFile(actorDB.Photo, _container);
            }

            _context.Remove(actorDB);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}