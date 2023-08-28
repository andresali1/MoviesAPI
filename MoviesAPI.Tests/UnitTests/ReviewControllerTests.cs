using Microsoft.AspNetCore.Mvc.Infrastructure;
using MoviesAPI.DTOs;
using MoviesAPI.Migrations;

namespace MoviesAPI.Tests.UnitTests
{
    [TestClass]
    public class ReviewControllerTests : TestBase
    {
        /// <summary>
        /// Method to save one movie in the testing db
        /// </summary>
        /// <param name="bdName">Name of the instance of the testin db</param>
        /// <returns></returns>
        private async Task CreateMovies(string bdName)
        {
            var context = BuildContext(bdName);

            context.Movie.Add(new Movie() { Title = "Movie 1" });

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Testing => Getting Reviews by Filter
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Get()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();
            await CreateMovies(bdName);

            var movieId = await context.Movie.Select(x => x.Id).FirstAsync();
            var reviews = new List<Review>() {
                new Review() { MovieId = movieId, AppUserId = defaultUserId, Score = 5 },
                new Review() { MovieId = movieId, AppUserId = "7b34f686-2bda-4729-a827-2f02f24e9539", Score = 1 },
                new Review() { MovieId = movieId, AppUserId = "752d3e6a-4632-4684-9bbd-0f30192b7a2e", Score = 5 },
                new Review() { MovieId = movieId, AppUserId = "92b70fea-205c-4878-be93-4bda52ebae0e", Score = 3 }
            };

            var paginationDTO = new PaginationDTO() { Page = 1, RecordsPerPage = 2 };

            context.Review.AddRange(reviews);
            await context.SaveChangesAsync();

            var context2 = BuildContext(bdName);

            // Test
            var controller = new ReviewController(context2, mapper);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var response = await controller.Get(movieId, paginationDTO);

            // Verification
            var result = response.Value;
            Assert.AreEqual(2, result.Count);
        }

        /// <summary>
        /// Testing => Ger error by trying to create two reviews to the same movie
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PostTwiceToSameMovie()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();
            await CreateMovies(bdName);

            var movieId = await context.Movie.Select(x => x.Id).FirstAsync();
            var review1 = new Review() { MovieId = movieId, AppUserId = defaultUserId, Score = 5 };
            var reviewDTO = new ReviewCreationDTO() { Comment = "good", Score = 4 };

            context.Add(review1);
            await context.SaveChangesAsync();

            var context2 = BuildContext(bdName);

            // Test
            var controller = new ReviewController(context2, mapper);
            controller.ControllerContext = BuildControllerContext();

            var response = await controller.Post(movieId, reviewDTO);

            // Verification
            var result = response as IStatusCodeActionResult;
            Assert.AreEqual(400, result.StatusCode.Value);
        }

        /// <summary>
        /// Testing => Create a new Review in Db
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Post()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();
            await CreateMovies(bdName);

            var movieId = await context.Movie.Select(x => x.Id).FirstAsync();
            var reviewDTO = new ReviewCreationDTO() { Comment = "good", Score = 4 };

            var context2 = BuildContext(bdName);
            var context3 = BuildContext(bdName);

            // Test
            var controller = new ReviewController(context2, mapper);
            controller.ControllerContext = BuildControllerContext();

            var response = await controller.Post(movieId, reviewDTO);

            // Verification
            var result = response as NoContentResult;
            Assert.IsNotNull(result);

            var exists = await context3.Review.AnyAsync(r => r.Comment.Equals(reviewDTO.Comment));
            Assert.IsTrue(exists);
        }

        /// <summary>
        /// Testing => Getting 404 status code by trying to update a non existent Review
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutNonExistentId()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();
            await CreateMovies(bdName);

            var movieId = await context.Movie.Select(x => x.Id).FirstAsync();

            // Test
            var controller = new ReviewController(context, mapper);
            var response = await controller.Put(movieId, 1, new ReviewCreationDTO() { Score = 2 });

            // Verification
            var result = response as StatusCodeResult;
            Assert.AreEqual(404, result.StatusCode);
        }

        /// <summary>
        /// Testing => Getting error by trying to update a review of another user
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutToDifferentUserReview()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();
            await CreateMovies(bdName);

            var movieId = await context.Movie.Select(x => x.Id).FirstAsync();
            var reviews = new List<Review>() {
                new Review() { MovieId = movieId, AppUserId = defaultUserId, Score = 5 },
                new Review() { MovieId = movieId, AppUserId = "7b34f686-2bda-4729-a827-2f02f24e9539", Score = 1 },
            };

            var reviewDTO = new ReviewCreationDTO() { Comment = "Good", Score = 3 };

            context.Review.AddRange(reviews);
            await context.SaveChangesAsync();

            var context2 = BuildContext(bdName);
            var context3 = BuildContext(bdName);

            // Test
            var controller = new ReviewController(context2, mapper);
            controller.ControllerContext = BuildControllerContext();

            int id = 2;
            var response = await controller.Put(movieId, id, reviewDTO);

            // Verification
            var result = response as StatusCodeResult;
            Assert.AreEqual(null, result);
        }

        /// <summary>
        /// Testing => Updating a Review in DB
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutExistentId()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();
            await CreateMovies(bdName);

            var movieId = await context.Movie.Select(x => x.Id).FirstAsync();
            var reviews = new List<Review>() {
                new Review() { MovieId = movieId, AppUserId = defaultUserId, Score = 5 },
                new Review() { MovieId = movieId, AppUserId = "7b34f686-2bda-4729-a827-2f02f24e9539", Score = 1 },
            };

            var reviewDTO = new ReviewCreationDTO() { Comment = "Good", Score = 3 };

            context.Review.AddRange(reviews);
            await context.SaveChangesAsync();

            var context2 = BuildContext(bdName);
            var context3 = BuildContext(bdName);

            // Test
            var controller = new ReviewController(context2, mapper);
            controller.ControllerContext = BuildControllerContext();

            int id = 1;
            var response = await controller.Put(movieId, id, reviewDTO);

            // Verification
            var result = response as StatusCodeResult;
            Assert.AreEqual(204, result.StatusCode);

            var reviewDb = await context3.Review.FirstAsync(r => r.Id == id);
            Assert.AreEqual(reviewDTO.Comment, reviewDb.Comment);
            Assert.AreNotEqual(5, reviewDb.Score);
        }

        /// <summary>
        /// Testing => Getting 404 status code by trying to delete a non existent Review
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DeleteNonExistentId()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();
            await CreateMovies(bdName);

            var movieId = await context.Movie.Select(x => x.Id).FirstAsync();

            // Test
            var controller = new ReviewController(context, mapper);
            controller.ControllerContext = BuildControllerContext();

            int id = 1;
            var response = await controller.Delete(movieId, id);

            // Verification
            var result = response as StatusCodeResult;
            Assert.AreEqual(404, result.StatusCode);
        }

        /// <summary>
        /// Testing => Deleting a Review from DB
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DeleteExistentId()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();
            await CreateMovies(bdName);

            var movieId = await context.Movie.Select(x => x.Id).FirstAsync();
            var reviews = new List<Review>() {
                new Review() { MovieId = movieId, AppUserId = defaultUserId, Score = 5 },
                new Review() { MovieId = movieId, AppUserId = "7b34f686-2bda-4729-a827-2f02f24e9539", Score = 1 },
            };

            context.Review.AddRange(reviews);
            await context.SaveChangesAsync();

            var context2 = BuildContext(bdName);
            var context3 = BuildContext(bdName);

            // Test
            var controller = new ReviewController(context2, mapper);
            controller.ControllerContext = BuildControllerContext();

            int id = 1;
            var response = await controller.Delete(movieId, id);

            // Verification
            var result = response as StatusCodeResult;
            Assert.AreEqual(204, result.StatusCode);

            var exists = await context3.Review.AnyAsync(r => r.AppUserId.Equals(defaultUserId));
            Assert.IsFalse(exists);
        }
    }
}