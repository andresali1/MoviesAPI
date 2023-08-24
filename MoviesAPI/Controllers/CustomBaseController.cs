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
    public class CustomBaseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CustomBaseController(ApplicationDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Method to get a list of records of any entity in DB
        /// </summary>
        /// <typeparam name="TEntity">Entity to get</typeparam>
        /// <typeparam name="TDto">DTO to be mapped by the entity</typeparam>
        /// <returns></returns>
        protected async Task<List<TDto>> Get<TEntity, TDto>() where TEntity : class
        {
            var entities = await _context.Set<TEntity>().AsNoTracking().ToListAsync();
            var dtos = _mapper.Map<List<TDto>>(entities);
            return dtos;
        }

        /// <summary>
        /// Method to get a list of records of any entity in DB using pagination 
        /// </summary>
        /// <typeparam name="TEntity">Entity to get</typeparam>
        /// <typeparam name="TDto">DTO to be mapped by the entity</typeparam>
        /// <param name="paginationDTO">Object with the pagination params</param>
        /// <returns></returns>
        protected async Task<List<TDto>> Get<TEntity, TDto>(PaginationDTO paginationDTO) where TEntity : class
        {
            var queryable = _context.Set<TEntity>().AsQueryable();
            return await Get<TEntity, TDto>(paginationDTO, queryable);
        }

        /// <summary>
        /// Method to get a list of records of any entity in DB using pagination and IQueryable
        /// </summary>
        /// <typeparam name="TEntity">Entity to get</typeparam>
        /// <typeparam name="TDto">DTO to be mapped by the entity</typeparam>
        /// <param name="paginationDTO">Object with the pagination params</param>
        /// <param name="queryable">Entity data as Queryable</param>
        /// <returns></returns>
        protected async Task<List<TDto>> Get<TEntity, TDto>(PaginationDTO paginationDTO, IQueryable<TEntity> queryable) where TEntity : class
        {
            await HttpContext.InsertPaginationParams(queryable, paginationDTO.RecordsPerPage);
            var entities = await queryable.Page(paginationDTO).ToListAsync();
            return _mapper.Map<List<TDto>>(entities);
        }

        /// <summary>
        /// Method to get a record of any Entity in DB that implements IId
        /// </summary>
        /// <typeparam name="TEntity">Entity to get</typeparam>
        /// <typeparam name="TDto">DTO to be mapped by the entity</typeparam>
        /// <param name="id">Id of the record</param>
        /// <returns></returns>
        protected async Task<ActionResult<TDto>> Get<TEntity, TDto>(int id) where TEntity : class, IId
        {
            var entity = await _context.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound();
            }

            return _mapper.Map<TDto>(entity);
        }

        /// <summary>
        /// Method to create a new record of any Entity in DB that implements IId
        /// </summary>
        /// <typeparam name="TCreation">DTO Object with the data to create</typeparam>
        /// <typeparam name="TEntity">Entity to save</typeparam>
        /// <typeparam name="TRead">DTO Object to be returned</typeparam>
        /// <param name="creationDTO">DTO object with the data to save</param>
        /// <param name="routeName">Name of the route</param>
        /// <returns></returns>
        protected async Task<ActionResult> Post<TCreation, TEntity, TRead>(TCreation creationDTO, string routeName) where TEntity : class, IId
        {
            var entity = _mapper.Map<TEntity>(creationDTO);
            _context.Add(entity);
            await _context.SaveChangesAsync();
            var readDTO = _mapper.Map<TRead>(entity);

            return new CreatedAtRouteResult(routeName, new { id = entity.Id }, readDTO);
        }

        /// <summary>
        /// Method to update a record of any Entity in DB that implements IId
        /// </summary>
        /// <typeparam name="TCreation">DTO Object with the data to update</typeparam>
        /// <typeparam name="TEntity">Entity to save</typeparam>
        /// <param name="id">Id of the record</param>
        /// <param name="creationDTO">DTO object with the data to save</param>
        /// <returns></returns>
        protected async Task<ActionResult> Put<TCreation, TEntity>(int id, TCreation creationDTO) where TEntity : class, IId
        {
            var exists = await _context.Set<TEntity>().AnyAsync(x => x.Id == id);

            if (!exists)
            {
                return NotFound();
            }

            var entity = _mapper.Map<TEntity>(creationDTO);
            entity.Id = id;
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Method to patch a record of any Entity in DB that implements IId
        /// </summary>
        /// <typeparam name="TEntity">Entity to save</typeparam>
        /// <typeparam name="TDto"></typeparam>
        /// <param name="id"></param>
        /// <param name="patchDocument"></param>
        /// <returns></returns>
        protected async Task<ActionResult> Patch<TEntity, TDto>(int id, JsonPatchDocument<TDto> patchDocument)
            where TDto : class
            where TEntity : class, IId
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var entityDB = await _context.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == id);

            if (entityDB == null)
            {
                return NotFound();
            }

            var entityDTO = _mapper.Map<TDto>(entityDB);

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
        /// Method to delete a record on any Entity in Db that implements IId
        /// </summary>
        /// <typeparam name="TEntity">Entity to save</typeparam>
        /// <param name="id">Id of the record</param>
        /// <returns></returns>
        protected async Task<ActionResult> Delete<TEntity>(int id) where TEntity : class, IId, new()
        {
            var exists = await _context.Set<TEntity>().AnyAsync(x => x.Id == id);

            if (!exists)
            {
                return NotFound();
            }

            _context.Remove(new TEntity() { Id = id });
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
