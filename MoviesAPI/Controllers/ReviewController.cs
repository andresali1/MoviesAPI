using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using MoviesAPI.Helpers;
using MoviesAPI.Migrations;
using System.Security.Claims;

namespace MoviesAPI.Controllers
{
    [ApiController]
    [Route("api/movie/{movieId:int}/review")]
    [ServiceFilter(typeof(MovieExistsAttribute))]
    public class ReviewController : CustomBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ReviewController(ApplicationDbContext context,
            IMapper mapper)
            : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Method to geat al the reviews of one movie by pagination params
        /// </summary>
        /// <param name="paginationDTO">Object with pagination params</param>
        /// <returns></returns>
        [HttpGet(Name = "getReviews")]
        public async Task<ActionResult<List<ReviewDTO>>> Get(int movieId, [FromQuery] PaginationDTO paginationDTO)
        {
            var queryable = _context.Review.Include(r => r.AppUser).AsQueryable();
            queryable = queryable.Where(q => q.MovieId == movieId);
            return await Get<Review, ReviewDTO>(paginationDTO, queryable);
        }

        /// <summary>
        /// Method to create a new Review in DB
        /// </summary>
        /// <param name="movieId">Id of the movie to review</param>
        /// <param name="reviewCreationDTO">Object with review info to create</param>
        /// <returns></returns>
        [HttpPost(Name = "createReview")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post(int movieId, [FromBody] ReviewCreationDTO reviewCreationDTO)
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            var reviewExists = await _context.Review.AnyAsync(r => r.MovieId == movieId && r.AppUserId == userId);

            if(reviewExists)
            {
                return BadRequest("El usuario ya ha escrito un review de ésta película");
            }

            var review = _mapper.Map<Review>(reviewCreationDTO);
            review.MovieId = movieId;
            review.AppUserId = userId;

            _context.Add(review);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Method to update a review in DB
        /// </summary>
        /// <param name="movieId">Id of the movie</param>
        /// <param name="reviewId">Id of he review</param>
        /// <param name="reviewCreationDTO">Object with review info to update</param>
        /// <returns></returns>
        [HttpPut("{reviewId:int}", Name = "putReview")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Put(int movieId, int reviewId, [FromBody] ReviewCreationDTO reviewCreationDTO)
        {
            var reviewDB = await _context.Review.FirstOrDefaultAsync(r => r.Id == reviewId);

            if(reviewDB == null) { return NotFound(); }

            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if(reviewDB.AppUserId != userId)
            {
                return BadRequest("No tiene permisos para editar este review");
            }

            reviewDB = _mapper.Map(reviewCreationDTO, reviewDB);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Method to delete a Review from DB
        /// </summary>
        /// <param name="movieId">Id of the movie</param>
        /// <param name="reviewId">Id of the review</param>
        /// <returns></returns>
        [HttpDelete("{reviewId:int}", Name = "deleteReview")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Delete(int movieId, int reviewId)
        {
            var reviewDB = await _context.Review.FirstOrDefaultAsync(r => r.Id == reviewId);

            if (reviewDB == null) { return NotFound(); }

            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (reviewDB.AppUserId != userId) { return Forbid(); }

            _context.Remove(reviewDB);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
