using Newtonsoft.Json;

namespace MoviesAPI.Tests.IntegrationTests
{
    [TestClass]
    public class GenreControllerTests : TestBase
    {
        public static readonly string url = "api/genre";

        /// <summary>
        /// Testing => Get Empty list when genre table is empty
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetAllEmptyList()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var factory = BuildWebApplicationFactory(bdName);

            // Test
            var client = factory.CreateClient();
            var response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            // Verification
            var genres = JsonConvert.DeserializeObject<List<GenreDTO>>(await response.Content.ReadAsStringAsync());
            Assert.AreEqual(0, genres.Count);
        }

        /// <summary>
        /// Testing => Get genres list
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetAll()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var factory = BuildWebApplicationFactory(bdName);

            var context = BuildContext(bdName);
            context.Genre.Add(new Genre() { G_Name = "Genre 1" });
            context.Genre.Add(new Genre() { G_Name = "Genre 2" });
            await context.SaveChangesAsync();

            // Test
            var client = factory.CreateClient();
            var response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            // Verification
            var genres = JsonConvert.DeserializeObject<List<GenreDTO>>(await response.Content.ReadAsStringAsync());
            Assert.AreEqual(2, genres.Count);
        }

        /// <summary>
        /// Testing => Getting 401 status code by trying ot delete a Genre without Admin permission
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DeleteReturn401()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var factory = BuildWebApplicationFactory(bdName, ignoreSecurity: false);

            // Test
            var client = factory.CreateClient();
            var response = await client.DeleteAsync($"{url}/1");

            // Verification
            Assert.AreEqual("Unauthorized", response.ReasonPhrase);
        }

        /// <summary>
        /// Testing => Deleting a Genre in DB
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Delete()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var factory = BuildWebApplicationFactory(bdName);

            var context = BuildContext(bdName);
            context.Genre.Add(new Genre() { G_Name = "Genre 1" });
            await context.SaveChangesAsync();

            var context2 = BuildContext(bdName);

            // Test
            var client = factory.CreateClient();
            var response = await client.DeleteAsync($"{url}/1");

            response.EnsureSuccessStatusCode();

            // Verification
            var exists = await context2.Genre.AnyAsync(g => g.G_Name.Equals("Genre 1"));
            Assert.IsFalse(exists);
        }
    }
}