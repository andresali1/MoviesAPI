using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using MoviesAPI.Helpers;

namespace MoviesAPI.Tests
{
    public class TestBase
    {
        /// <summary>
        /// Method to create a custom database in memory to do testing
        /// </summary>
        /// <param name="dbName">Given name of this instance of the new inmemory database</param>
        /// <returns></returns>
        protected ApplicationDbContext BuildContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName).Options;

            var dbContext = new ApplicationDbContext(options);

            return dbContext;
        }

        /// <summary>
        /// Method to configure an automapper instance for testing
        /// </summary>
        /// <returns></returns>
        protected IMapper ConfigureAutoMapper()
        {
            var config = new MapperConfiguration(options =>
            {
                var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
                options.AddProfile(new AutoMapperProfiles(geometryFactory));
            });

            return config.CreateMapper();
        }
    }
}
