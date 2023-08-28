using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace MoviesAPI.Tests.UnitTests
{
    [TestClass]
    public class CinemaControllerTests : TestBase
    {
        /// <summary>
        /// Testing => Get all the cinemas in Db
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Get()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            context.Cinema.Add(new Cinema() { C_Name = "Cinema 1" });
            context.Cinema.Add(new Cinema() { C_Name = "Cinema 2" });
            context.Cinema.Add(new Cinema() { C_Name = "Cinema 3" });
            await context.SaveChangesAsync();

            var context2 = BuildContext(bdName);

            // Test
            var controller = new CinemaController(context, mapper, null);
            var response = await controller.Get();

            // Verification
            var result = response.Value;
            Assert.AreEqual(3, result.Count);
        }

        /// <summary>
        /// Testing => Getting 404 status code by trying to get a non existent Cinema
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
            var controller = new CinemaController(context, mapper, null);
            var response = await controller.Get(1);

            // Verification
            var result = response.Result as StatusCodeResult;
            Assert.AreEqual(404, result.StatusCode);
        }

        /// <summary>
        /// Testing => Get a Cinema by its Id
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetByExistentId()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            context.Cinema.Add(new Cinema() { C_Name = "Cinema 1" });
            context.Cinema.Add(new Cinema() { C_Name = "Cinema 2" });
            await context.SaveChangesAsync();

            var context2 = BuildContext(bdName);

            // Test
            var controller = new CinemaController(context2, mapper, null);
            var response = await controller.Get(1);

            // Verification
            var result = response.Value;
            Assert.IsNotNull(result);
            Assert.AreEqual("Cinema 1", result.C_Name);
        }

        /// <summary>
        /// Testing => Get all the cinemas according to nearby filter
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetNearBy()
        {
            // Preparation
            List<NearCinemaDTO> value = new List<NearCinemaDTO>();

            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            using (var context = LocalDbDataBaseInitializer.GetDbContextLocalDb(false))
            {
                var cinemas = new List<Cinema>()
                {
                    new Cinema() { C_Name = "Unicentro", Location = geometryFactory.CreatePoint(new Coordinate(-74.042149, 4.702402)) },
                    new Cinema() { C_Name = "Iserra", Location = geometryFactory.CreatePoint(new Coordinate(-74.064697, 4.687945)) },
                    new Cinema() { C_Name = "Andino", Location = geometryFactory.CreatePoint(new Coordinate(-74.052658, 4.667265)) },
                    new Cinema() { C_Name = "Santa Fe", Location = geometryFactory.CreatePoint(new Coordinate(-74.044943, 4.762310)) }
                };

                context.AddRange(cinemas);
                await context.SaveChangesAsync();
            }

            //using this coordinates you'll get only 1 Cinema less than 2Km away (If you change the coordinates, the result will be different)
            var filter = new NearCinemaFilterDTO() { DistanceInKm = 2, Latitude = 4.680024, Longitude = -74.041616 };

            // Test
            using (var context = LocalDbDataBaseInitializer.GetDbContextLocalDb(false))
            {
                var mapper = ConfigureAutoMapper();
                var controller = new CinemaController(context, mapper, geometryFactory);
                var response = await controller.Get(filter);
                value = response.Value;
            }

            // Verification
            Assert.AreEqual(1, value.Count);
        }

        /// <summary>
        /// Testing => Create a new Cinema
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Post()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var context = BuildContext(bdName);
            var mapper = ConfigureAutoMapper();

            var cinemaDTO = new CinemaCreationDTO() { C_Name = "Cinema 1" };

            var context2 = BuildContext(bdName);

            // Test
            var controller = new CinemaController(context, mapper, null);
            var response = await controller.Post(cinemaDTO);

            // Verification
            var result = response as CreatedAtRouteResult;
            Assert.AreEqual(201, result.StatusCode);

            var exists = await context2.Cinema.AnyAsync(c => c.C_Name.Equals(cinemaDTO.C_Name));
            Assert.IsTrue(exists);
        }
    }
}