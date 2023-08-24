using Microsoft.AspNetCore.Mvc;

namespace MoviesAPI.Tests.UnitTests
{
    [TestClass]
    public class GenreControllerTests : TestBase
    {
        /// <summary>
        /// Testing => Getting All Genres
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetAll()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            context.Genre.Add(new Genre() { G_Name = "Genre 1" });
            context.Genre.Add(new Genre() { G_Name = "Genre 2" });

            await context.SaveChangesAsync();

            var context2 = BuildContext(bdName);

            // Test
            var controller = new GenreController(context2, mapper);
            var response = await controller.Get();

            // Verification
            var genres = response.Value;
            Assert.AreEqual(2, genres.Count());
        }

        /// <summary>
        /// Testing => Getting error by non existent Genre 
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetByNonExistentId()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            // Test
            var controller = new GenreController(context, mapper);
            var response = await controller.Get(1);

            // Verification
            var result = response.Result as StatusCodeResult;
            Assert.AreEqual(404, result.StatusCode);
        }
    }
}



// Preparation
// Test
// Verification
