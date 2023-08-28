using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using MoviesAPI.Helpers;
using System.Security.Claims;

namespace MoviesAPI.Tests
{
    public class TestBase
    {
        protected string defaultUserId = "6ea18f22-2b4f-49a3-bccb-6603e2b72617";
        protected string defaultUserEmail = "test@gmail.com";

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

        /// <summary>
        /// Utility method to create a default Claim object to the httpcontent
        /// </summary>
        /// <returns></returns>
        protected ControllerContext BuildControllerContext()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] 
            {
                new Claim(ClaimTypes.Name, defaultUserEmail),
                new Claim(ClaimTypes.Email, defaultUserEmail),
                new Claim(ClaimTypes.NameIdentifier, defaultUserId)
            }));

            return new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }
    }
}
