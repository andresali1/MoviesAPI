using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        /// Method to get a record of any Entity in DB that implements IId
        /// </summary>
        /// <typeparam name="TEntity">Entity to get</typeparam>
        /// <typeparam name="TDto">DTO to be mapped by the entity</typeparam>
        /// <param name="id">Id of the record</param>
        /// <returns></returns>
        protected async Task<ActionResult<TDto>> Get<TEntity, TDto>(int id) where TEntity : class, IId
        {
            var entity = await _context.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

            if(entity == null)
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
    }
}
