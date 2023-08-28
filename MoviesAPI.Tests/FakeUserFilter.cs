using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace MoviesAPI.Tests
{
    public class FakeUserFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.Email, "example@gmail.com"),
                new Claim(ClaimTypes.Name, "example@gmail.com"),
                new Claim(ClaimTypes.NameIdentifier, "be74d07b-ce2e-4c20-9ce6-1c3047a16f5f")
            }, "test"));

            await next();
        }
    }
}
