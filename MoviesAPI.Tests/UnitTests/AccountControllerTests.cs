using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MoviesAPI.Tests.UnitTests
{
    [TestClass]
    public class AccountControllerTests : TestBase
    {
        /// <summary>
        /// Method to help to create a new user
        /// </summary>
        /// <param name="dbName">Name of the instance of the db</param>
        /// <returns></returns>
        private async Task CreateUserHelper(string dbName)
        {
            var accountController = BuildAccountController(dbName);
            var userInfo = new UserInfo() { Email = "test@gmail.com", Password = "Test123." };
            await accountController.CreateUser(userInfo);
        }

        /// <summary>
        /// Method to Build an Account Controller for testing
        /// </summary>
        /// <param name="dbName">Name of the instance of the db</param>
        /// <returns></returns>
        private AccountController BuildAccountController(string dbName)
        {
            var context = BuildContext(dbName);
            var miUserStore = new UserStore<IdentityUser>(context);
            var userManager = BuildUserManager(miUserStore);
            var mapper = ConfigureAutoMapper();

            var httpContext = new DefaultHttpContext();
            MockAuth(httpContext);
            var signInManager = SetupSignInManager(userManager, httpContext);

            var myConfiguration = new Dictionary<string, string>
            {
                {"JWT:key", "ALSPQWLSLWRIGJHVNFDGTOEIUDHFVNWPDOFKJCMRPOQDQWQSXQWJFCMRICMQEOIRFUCHWMROKFWOEPIRM" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            return new AccountController(userManager, signInManager, configuration, context, mapper);
        }

        // Source: https://github.com/dotnet/aspnetcore/blob/master/src/Identity/test/Shared/MockHelpers.cs
        // Source: https://github.com/dotnet/aspnetcore/blob/master/src/Identity/test/Identity.Test/SignInManagerTest.cs
        // Some code was modified to be adapted to our project.
        private UserManager<TUser> BuildUserManager<TUser>(IUserStore<TUser> store = null) where TUser : class
        {
            store = store ?? new Mock<IUserStore<TUser>>().Object;
            var options = new Mock<IOptions<IdentityOptions>>();
            var idOptions = new IdentityOptions();
            idOptions.Lockout.AllowedForNewUsers = false;

            options.Setup(o => o.Value).Returns(idOptions);

            var userValidators = new List<IUserValidator<TUser>>();

            var validator = new Mock<IUserValidator<TUser>>();
            userValidators.Add(validator.Object);
            var pwdValidators = new List<PasswordValidator<TUser>>();
            pwdValidators.Add(new PasswordValidator<TUser>());

            var userManager = new UserManager<TUser>(store, options.Object, new PasswordHasher<TUser>(),
                userValidators, pwdValidators, new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(), null,
                new Mock<ILogger<UserManager<TUser>>>().Object);

            validator.Setup(v => v.ValidateAsync(userManager, It.IsAny<TUser>()))
                .Returns(Task.FromResult(IdentityResult.Success)).Verifiable();

            return userManager;
        }

        private static SignInManager<TUser> SetupSignInManager<TUser>(UserManager<TUser> manager,
            HttpContext context, ILogger logger = null, IdentityOptions identityOptions = null,
            IAuthenticationSchemeProvider schemeProvider = null) where TUser : class
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            contextAccessor.Setup(a => a.HttpContext).Returns(context);
            identityOptions = identityOptions ?? new IdentityOptions();
            var options = new Mock<IOptions<IdentityOptions>>();
            options.Setup(a => a.Value).Returns(identityOptions);
            var claimsFactory = new UserClaimsPrincipalFactory<TUser>(manager, options.Object);
            schemeProvider = schemeProvider ?? new Mock<IAuthenticationSchemeProvider>().Object;
            var sm = new SignInManager<TUser>(manager, contextAccessor.Object, claimsFactory, options.Object, null, schemeProvider, new DefaultUserConfirmation<TUser>());
            sm.Logger = logger ?? (new Mock<ILogger<SignInManager<TUser>>>()).Object;
            return sm;
        }

        /// <summary>
        /// Method to create a Mock of an IAuthentication servie
        /// </summary>
        /// <param name="context">given HttpContext</param>
        /// <returns></returns>
        private Mock<IAuthenticationService> MockAuth(HttpContext context)
        {
            var auth = new Mock<IAuthenticationService>();
            context.RequestServices = new ServiceCollection().AddSingleton(auth.Object).BuildServiceProvider();
            return auth;
        }

        /// <summary>
        /// Testing => Create a new user
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Create()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            var contex = BuildContext(bdName);

            // Test
            await CreateUserHelper(bdName);

            // Verification
            var count = await contex.Users.CountAsync();
            Assert.AreEqual(1, count);
        }

        /// <summary>
        /// Testing => Getting an error by trying to login with incorrect credentials
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task FailedLogin()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            await CreateUserHelper(bdName);

            var userInfo = new UserInfo() { Email = "test@gmail.com", Password = "wrongpassword" };

            // Test
            var controller = BuildAccountController(bdName);
            var response = await controller.Login(userInfo);

            // Verification
            Assert.IsNull(response.Value);

            var result = response.Result as BadRequestObjectResult;
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Testing => Login an User
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task SucceddedLogin()
        {
            // Preparation
            var bdName = Guid.NewGuid().ToString();
            await CreateUserHelper(bdName);

            var userInfo = new UserInfo() { Email = "test@gmail.com", Password = "Test123." };

            // Test
            var controller = BuildAccountController(bdName);
            var response = await controller.Login(userInfo);

            // Verification
            Assert.IsNotNull(response.Value);
            Assert.IsNotNull(response.Value.Token);
        }
    }
}