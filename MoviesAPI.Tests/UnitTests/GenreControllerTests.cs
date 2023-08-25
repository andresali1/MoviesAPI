using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.DTOs;

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
        /// Testing => Getting a 404 Status code by trying to get a non existent Genre 
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

        /// <summary>
        /// Testing => Getting a genre by its id
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetByExistentId()
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

            var id = 1;
            var response = await controller.Get(id);

            // Verification
            var result = response.Value;
            Assert.AreEqual(id, result.Id);
        }

        /// <summary>
        /// Testing => Creating a Genre
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Create()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            var newGenre = new GenreCreationDTO() { G_Name = "New Genre" };

            // Test
            var controller = new GenreController(context, mapper);
            var response = await controller.Post(newGenre);

            // Verification
            var result = response as CreatedAtRouteResult;
            Assert.IsNotNull(result);

            var context2 = BuildContext(bdName);
            var amount = await context2.Genre.CountAsync();
            Assert.AreEqual(1, amount);
        }

        /// <summary>
        /// Testing => Updating a Genre with Put
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Put()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            context.Genre.Add(new Genre() { G_Name = "Genre 1" });
            await context.SaveChangesAsync();

            // Test
            var context2 = BuildContext(bdName);
            var controller = new GenreController(context2, mapper);

            var genreCreationDTO = new GenreCreationDTO() { G_Name = "Updated Name" };

            var id = 1;
            var response = await controller.Put(id, genreCreationDTO);

            // Verification
            var result = response as StatusCodeResult;
            Assert.AreEqual(204, result.StatusCode);

            var context3 = BuildContext(bdName);
            var exists = await context3.Genre.AnyAsync(g => g.G_Name.Equals(genreCreationDTO.G_Name));
            Assert.IsTrue(exists);
        }

        /// <summary>
        /// Testing => Getting a 404 Status code by trying to delete a non existent Genre
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
            var controller = new GenreController(context, mapper);

            var response = await controller.Delete(1);

            // Verification
            var result = response as StatusCodeResult;
            Assert.AreEqual(404, result.StatusCode);
        }

        /// <summary>
        /// Testing => Delete an existing Genre
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DeleteExistentId()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            var newGenre = new Genre() { G_Name = "Genre 1" };

            context.Genre.Add(newGenre);
            await context.SaveChangesAsync();

            // Test
            var context2 = BuildContext(bdName);
            var controller = new GenreController(context2, mapper);

            var response = await controller.Delete(1);

            // Verification
            var result = response as StatusCodeResult;
            Assert.AreEqual(204, result.StatusCode);

            var context3 = BuildContext(bdName);
            var exists = await context3.Genre.AnyAsync(g => g.G_Name.Equals(newGenre.G_Name));
            Assert.IsFalse(exists);
        }
    }
}



// Preparation
// Test
// Verification
