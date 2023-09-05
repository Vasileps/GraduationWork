using MessengerServer.Services.Interfaces;
using Microsoft.Extensions.Primitives;

namespace MessengerServer.Middleware
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IJWTService jwt;

        public AuthMiddleware(IJWTService jwt, RequestDelegate next)
        {
            this.jwt = jwt;
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint is null)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("EndpointNotFound");
                return;
            }

            if (endpoint.Metadata.GetMetadata<AuthRequired>() is null)
            {
                await next.Invoke(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue("AccessToken", out StringValues accessTokens))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("TokenNotFound");
                return;
            }

            if (accessTokens.Count != 1)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("OnlyOneTokenAllowed");
                return;
            }

            var accessToken = accessTokens[0];

            if (!jwt.ValidateLifetime(accessToken))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("TokenExpired");
                return;
            }

            var userID = jwt.GetClaims(accessToken)?.SingleOrDefault(x => x.Type == "UserID");

            if (userID is null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("InvalidToken");
                return;
            }

            context.Items["UserID"] = userID.Value;
            await next.Invoke(context);
            return;
        }
    }
}
