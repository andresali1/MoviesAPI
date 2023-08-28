using System.Linq;

namespace MoviesAPI.Tests.UnitTests
{
    [TestClass]
    public class MovieControllerTests : TestBase
    {
        /// <summary>
        /// Utility method to create movie data for testing
        /// </summary>
        /// <returns></returns>
        private async Task<string> CreateTestData()
        {
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var genre = new Genre() { G_Name = "Genre 1" };

            var movies = new List<Movie>()
            {
                new Movie { Title = "Movie 1", RealeaseDate = new DateTime(2010, 1, 1), JustReleased = false },
                new Movie { Title = "Doesn't released movie", RealeaseDate = DateTime.Today.AddDays(1), JustReleased = false },
                new Movie { Title = "Movie in theaters", RealeaseDate = DateTime.Today.AddDays(-1), JustReleased = true }
            };

            var movieWithGenre = new Movie()
            {
                Title = "Movie with Genre",
                RealeaseDate = new DateTime(2010, 1, 1),
                JustReleased = false
            };

            movies.Add(movieWithGenre);

            context.Add(genre);
            context.AddRange(movies);
            await context.SaveChangesAsync();

            var movieGenre = new MovieGenre() { GenreId = genre.Id, MovieId = movieWithGenre.Id };
            context.Add(movieGenre);
            await context.SaveChangesAsync();

            return bdName;
        }

        /// <summary>
        /// Testing => Geting the latest movies
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetLatest()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            context.Movie.Add(new Movie() { Title = "Movie 1", RealeaseDate = DateTime.Now.AddDays(5), JustReleased = false });
            context.Movie.Add(new Movie() { Title = "Movie 2", RealeaseDate = DateTime.Now.AddDays(20), JustReleased = false });
            context.Movie.Add(new Movie() { Title = "Movie 3", RealeaseDate = DateTime.Now.AddDays(-5), JustReleased = true });
            context.Movie.Add(new Movie() { Title = "Movie 4", RealeaseDate = DateTime.Now.AddDays(-10), JustReleased = true });
            context.Movie.Add(new Movie() { Title = "Movie 5", RealeaseDate = DateTime.Now.AddDays(-15), JustReleased = true });
            context.Movie.Add(new Movie() { Title = "Movie 6", RealeaseDate = DateTime.Now.AddDays(-8), JustReleased = true });
            await context.SaveChangesAsync();

            var context2 = BuildContext(bdName);

            // Test
            var controller = new MovieController(context, mapper, null, null);
            var response = await controller.GetLatest();

            // Verification
            var result = response.Value;
            Assert.AreEqual(3, result.InTheaters.Count);
            Assert.AreEqual(2, result.ComingReleases.Count);
        }

        /// <summary>
        /// Testing => Get movies by their title
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task FilterByTitle()
        {
            // Preparation
            var bdName = await CreateTestData();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            var movieTitle = "Movie 1";

            var filterDTO = new MovieFilterDTO() { Title = movieTitle, RecordsPerPage = 10 };

            // Test
            var controller = new MovieController(context, mapper, null, null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            var response = await controller.Filter(filterDTO);

            // Verification
            var result = response.Value;
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(movieTitle, result[0].Title);
        }

        /// <summary>
        /// Testoing => Get only just released movies
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task FilterByJustReleased()
        {
            // Preparation
            var bdName = await CreateTestData();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            var filterDTO = new MovieFilterDTO() { JustReleased = true };

            // Test
            var controller = new MovieController(context, mapper, null, null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            var response = await controller.Filter(filterDTO);

            // Verification
            var result = response.Value;
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(true, result[0].JustReleased);
            Assert.AreEqual("Movie in theaters", result[0].Title);
        }

        /// <summary>
        /// Testing => Get only coming releases
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task FilterByComingRelease()
        {
            // Preparation
            var bdName = await CreateTestData();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            var filterDTO = new MovieFilterDTO() { ComingRelease = true };

            // Test
            var controller = new MovieController(context, mapper, null, null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            var response = await controller.Filter(filterDTO);

            // Verification
            var result = response.Value;
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Doesn't released movie", result[0].Title);
        }

        /// <summary>
        /// Testing => Get movies by genre filter
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task FilterByGenre()
        {
            // Preparation
            var bdName = await CreateTestData();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            var genreId = context.Genre.Select(g => g.Id).First();

            var filterDTO = new MovieFilterDTO() { GenreId = genreId };

            // Test
            var controller = new MovieController(context, mapper, null, null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            var response = await controller.Filter(filterDTO);

            // Verification
            var result = response.Value;
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Movie with Genre", result[0].Title);
        }

        /// <summary>
        /// Testing => Get movies ordered by ascending
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task FilterOrderAscending()
        {
            // Preparation
            var bdName = await CreateTestData();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            var filterDTO = new MovieFilterDTO() { OrderField = "title", AscendingOrder = true };

            var context2 = BuildContext(bdName);

            // Test
            var controller = new MovieController(context, mapper, null, null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            var response = await controller.Filter(filterDTO);

            // Verification
            var result = response.Value;
            var moviesDB = await context2.Movie.OrderBy(m => m.Title).ToListAsync();

            Assert.AreEqual(moviesDB.Count, result.Count);

            for(int i = 0; i < moviesDB.Count; i++)
            {
                var resultMovie = result[i];
                var movieDb = moviesDB[i];

                Assert.AreEqual(resultMovie.Title, movieDb.Title);
            }
        }

        /// <summary>
        /// Testing => Get movies ordered by descending
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task FilterOrderDescending()
        {
            // Preparation
            var bdName = await CreateTestData();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            var filterDTO = new MovieFilterDTO() { OrderField = "realeaseDate", AscendingOrder = false };

            var context2 = BuildContext(bdName);

            // Test
            var controller = new MovieController(context, mapper, null, null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            var response = await controller.Filter(filterDTO);

            // Verification
            var result = response.Value;
            var moviesDB = await context2.Movie.OrderByDescending(m => m.RealeaseDate).ToListAsync();

            Assert.AreEqual(moviesDB.Count, result.Count);

            for (int i = 0; i < moviesDB.Count; i++)
            {
                var resultMovie = result[i];
                var movieDb = moviesDB[i];

                Assert.AreEqual(resultMovie.Title, movieDb.Title);
            }
        }

        /// <summary>
        /// Testing => Get 404 by sending a unexistent filter
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task FilterError()
        {
            // Preparation
            var bdName = await CreateTestData();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            var mock = new Mock<ILogger<MovieController>>();

            var filterDTO = new MovieFilterDTO() { OrderField = "aaa", AscendingOrder = true };

            var context2 = BuildContext(bdName);

            // Test
            var controller = new MovieController(context, mapper, null, mock.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            var response = await controller.Filter(filterDTO);

            // Verification
            var result = response.Value;
            var moviesDB = await context2.Movie.ToListAsync();

            Assert.AreEqual(moviesDB.Count, result.Count);

            Assert.AreEqual(1, mock.Invocations.Count);
        }

        /// <summary>
        /// Testiong => Get 404 status code by trying to get a non existent Movie
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetNonExistentId()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            // Test
            var controller = new MovieController(context, mapper, null ,null);
            var response = await controller.Get(1);

            // Verification
            var result = response.Result as StatusCodeResult;
            Assert.AreEqual(404, result.StatusCode);
        }

        /// <summary>
        /// Testing => Get a Movie by its Id
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetExistentId()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            var movie = new Movie() { Title = "Movie 1" };
            context.Movie.Add(movie);
            await context.SaveChangesAsync();

            var context2 = BuildContext(bdName);

            // Test
            var controller = new MovieController(context, mapper, null, null);

            int id = 1;
            var response = await controller.Get(id);

            // Verification
            var result = response.Value;
            Assert.AreEqual(id, result.Id);
            Assert.AreEqual(movie.Title, result.Title);
        }

        /// <summary>
        /// Testing => Creating a new Movie without Poster
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PostWithoutPoster()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            var movie = new MovieCreationDTO() { Title = "Movie 1", JustReleased = false, RealeaseDate = DateTime.Now };

            var mock = new Mock<IStoreFile>();
            mock.Setup(x => x.SaveFile(null, null, null, null))
                .Returns(Task.FromResult("url"));

            var context2 = BuildContext(bdName);

            // Test
            var controller = new MovieController(context, mapper, mock.Object, null);
            var response = await controller.Post(movie);

            // Verification
            var result = response as CreatedAtRouteResult;
            Assert.AreEqual(201, result.StatusCode);

            var movieDb = await context2.Movie.FirstOrDefaultAsync();
            Assert.IsNotNull(movieDb);
            Assert.AreEqual(movie.Title, movieDb.Title);

            Assert.AreEqual(0, mock.Invocations.Count);
        }

        /// <summary>
        /// Testing => Creating a new Movie with Poster
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PostWithPoster()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            var content = Encoding.UTF8.GetBytes("Test Image");
            var file = new FormFile(new MemoryStream(content), 0, content.Length, "Data", "image.jpg");
            file.Headers = new HeaderDictionary();
            file.ContentType = "image/jpg";

            var movie = new MovieCreationDTO() { Title = "Movie 1", JustReleased = false, RealeaseDate = DateTime.Now, Poster = file };

            var mock = new Mock<IStoreFile>();
            mock.Setup(x => x.SaveFile(content, ".jpg", "movies", file.ContentType))
                .Returns(Task.FromResult("url"));

            var context2 = BuildContext(bdName);

            // Test
            var controller = new MovieController(context, mapper, mock.Object, null);
            var response = await controller.Post(movie);

            // Verification
            var result = response as CreatedAtRouteResult;
            Assert.AreEqual(201, result.StatusCode);

            var movieDb = await context2.Movie.FirstOrDefaultAsync();
            Assert.IsNotNull(movieDb);
            Assert.AreEqual("url", movieDb.Poster);

            Assert.AreEqual(1, mock.Invocations.Count);
        }

        /// <summary>
        /// Testing => Get 404 by trying to update a non existent Movie
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutNonExistentId()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            var mock = new Mock<IStoreFile>();
            mock.Setup(x => x.SaveFile(null, null, null, null))
                .Returns(Task.FromResult("url"));

            // Test
            var controller = new MovieController(context, mapper, mock.Object, null);
            var response = await controller.Put(1, new MovieCreationDTO() { Title = "Movie 1" });

            // Verification
            var result = response as StatusCodeResult;
            Assert.AreEqual(404, result.StatusCode);
        }

        /// <summary>
        /// Testing => Update a Movie without sending a Poster
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutWithoutPoster()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            var movie = new Movie() { Title = "Movie 1", JustReleased = false, RealeaseDate = DateTime.Now.AddDays(1) };
            context.Movie.Add(movie);
            await context.SaveChangesAsync();

            var movieDTO = new MovieCreationDTO() { Title = "Edited Movie", JustReleased = true, RealeaseDate = DateTime.Now };

            var mock = new Mock<IStoreFile>();
            mock.Setup(x => x.SaveFile(null, null, null, null))
                .Returns(Task.FromResult("url"));

            var context2 = BuildContext(bdName);
            var context3 = BuildContext(bdName);

            // Test
            var controller = new MovieController(context2, mapper, mock.Object, null);

            int id = 1;
            var response = await controller.Put(id, movieDTO);

            // Verification
            var result = response as StatusCodeResult;
            Assert.AreEqual(204, result.StatusCode);

            var movieDB = await context3.Movie.FirstAsync(m => m.Id == id);
            Assert.AreNotEqual(movie.JustReleased, movieDB.JustReleased);

            Assert.AreEqual(0, mock.Invocations.Count);
        }

        /// <summary>
        /// Testing => Update a Movie sending a Poster
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PutWithPoster()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            var movie = new Movie() { Title = "Movie 1", JustReleased = false, RealeaseDate = DateTime.Now.AddDays(1) };
            context.Movie.Add(movie);
            await context.SaveChangesAsync();

            var content = Encoding.UTF8.GetBytes("Test Image");
            var file = new FormFile(new MemoryStream(content), 0, content.Length, "Data", "image.jpg");
            file.Headers = new HeaderDictionary();
            file.ContentType = "image/jpg";

            var movieDTO = new MovieCreationDTO() { Title = "Edited Movie", JustReleased = true, RealeaseDate = DateTime.Now, Poster = file };

            var mock = new Mock<IStoreFile>();
            mock.Setup(x => x.SaveFile(content, ".jpg", "movies", file.ContentType))
                .Returns(Task.FromResult("url"));

            var context2 = BuildContext(bdName);
            var context3 = BuildContext(bdName);

            // Test
            var controller = new MovieController(context2, mapper, mock.Object, null);

            int id = 1;
            var response = await controller.Put(id, movieDTO);

            // Verification
            var result = response as StatusCodeResult;
            Assert.AreEqual(204, result.StatusCode);

            var movieDb = await context3.Movie.FirstAsync(m => m.Id == id);
            Assert.AreNotEqual(movieDTO.RealeaseDate, movie.RealeaseDate);
            Assert.AreEqual(movieDTO.Title, movieDb.Title);

            Assert.AreEqual(1, mock.Invocations.Count);
        }

        /// <summary>
        /// Testing => Get 404 status code by trying to patch a non existent Movie
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PatchNonExistentId()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            var patchDocument = new JsonPatchDocument<MoviePatchDTO>();

            // Test
            var controller = new MovieController(context, mapper, null, null);
            var response = await controller.Patch(1, patchDocument);

            // Verification
            var result = response as StatusCodeResult;
            Assert.AreEqual(404, result.StatusCode);
        }

        /// <summary>
        /// Testing => Patch an existent Movie
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task PatchExistentId()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            var movie = new Movie() { Title = "Move 1", JustReleased = true, RealeaseDate = DateTime.Now };
            context.Movie.Add(movie);
            await context.SaveChangesAsync();

            var newTitle = "updated movie";
            var patchDocument = new JsonPatchDocument<MoviePatchDTO>();
            patchDocument.Operations.Add(new Operation<MoviePatchDTO>("replace", "/title", null, newTitle));

            var objectValidator = new Mock<IObjectModelValidator>();
            objectValidator.Setup(x => x.Validate(
                It.IsAny<ActionContext>(),
                It.IsAny<ValidationStateDictionary>(),
                It.IsAny<string>(),
                It.IsAny<object>()
            ));

            var context2 = BuildContext(bdName);
            var context3 = BuildContext(bdName);

            // Test
            var controller = new MovieController(context2, mapper, null, null);
            controller.ObjectValidator = objectValidator.Object;

            var response = await controller.Patch(1, patchDocument);

            // Verification
            var result = response as StatusCodeResult;
            Assert.AreEqual(204, result.StatusCode);

            var movieDb = await context3.Movie.FirstAsync();
            Assert.AreEqual(movie.RealeaseDate, movieDb.RealeaseDate);
            Assert.AreNotEqual(movie.Title, movieDb.Title);
            Assert.AreEqual(newTitle, movieDb.Title);
        }

        /// <summary>
        /// Testing => Get 404 status code by trying to delete a non existent movie
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DeleteNonExistentId()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            // Test
            var controller = new MovieController(context, mapper, null, null);
            var response = await controller.Delete(1);

            // Verification
            var result = response as StatusCodeResult;
            Assert.AreEqual(404, result.StatusCode);
        }

        /// <summary>
        /// Testing => Delete an existent Movie in DB
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DeleteExistentId()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            context.Movie.Add(new Movie() { Title = "Movie 1" });
            await context.SaveChangesAsync();

            var context2 = BuildContext(bdName);
            var context3 = BuildContext(bdName);

            // Test
            var controller = new MovieController(context2, mapper, null, null);

            var id = 1;
            var response = await controller.Delete(id);

            // Verification
            var result = response as StatusCodeResult;
            Assert.AreEqual(204, result.StatusCode);

            var exists = await context3.Movie.AnyAsync(m => m.Id == id);
            Assert.IsFalse(exists);
        }
    }
}