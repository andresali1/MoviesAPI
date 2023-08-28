using Newtonsoft.Json;
using System.Xml.Linq;

namespace MoviesAPI.Tests.IntegrationTests
{
    [TestClass]
    public class ReviewControllerTests : TestBase
    {
        public static readonly string url = "api/movie/1/review";

        /// <summary>
        /// Testing => Getting 404 when trying to get a review of non existent movie
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetAllReturns404WhenMovieDoesntExists()
        {
            // Preparation
            var dbName = Guid.NewGuid().ToString();
            var factory = BuildWebApplicationFactory(dbName);

            // Test
            var client = factory.CreateClient();
            var response = await client.GetAsync(url);

            // Verification
            Assert.AreEqual(404, (int)response.StatusCode);
        }

        /// <summary>
        /// Testing => Returns empty list when movie does exist but doesn´t have reviews
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetAllReturnsEmptyList()
        {
            // Preparation
            var dbName = Guid.NewGuid().ToString();
            var factory = BuildWebApplicationFactory(dbName);

            var context = BuildContext(dbName);
            context.Movie.Add(new Movie() { Title = "Movie 1", RealeaseDate = DateTime.Now, JustReleased = true });
            await context.SaveChangesAsync();

            // Test
            var client = factory.CreateClient();
            var response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            // Verification
            var reviews = JsonConvert.DeserializeObject<List<ReviewDTO>>(await response.Content.ReadAsStringAsync());
            Assert.AreEqual(0, reviews.Count);
        }
    }
}
