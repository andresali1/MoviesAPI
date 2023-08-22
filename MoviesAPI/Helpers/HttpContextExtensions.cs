using Microsoft.EntityFrameworkCore;

namespace MoviesAPI.Helpers
{
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Extension method to send the page amount to the headers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpContext"></param>
        /// <param name="queryable"></param>
        /// <param name="recordsByPageAmount"></param>
        /// <returns></returns>
        public async static Task InsertPaginationParams<T>(this HttpContext httpContext,
            IQueryable<T> queryable, int recordsByPageAmount)
        {
            double amount = await queryable.CountAsync();
            double pageAmount = Math.Ceiling(amount / recordsByPageAmount);
            httpContext.Response.Headers.Add("pageAmount", pageAmount.ToString());
        }
    }
}
