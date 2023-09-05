using MessengerServer.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;

namespace MessengerServer
{
    public class NotificationHub : Hub
    {
        private IJWTService jwt;

        public NotificationHub(IJWTService jwt)
        {
            this.jwt = jwt;
        }

        public override async Task OnConnectedAsync()
        {
            var context = Context.GetHttpContext();
            if (context is null) goto Abort;

            if (!context.Request.Headers.TryGetValue("AccessToken", out StringValues accessTokens))
                goto Abort;

            if (accessTokens.Count != 1) goto Abort;

            var accessToken = accessTokens[0];
            if (!jwt.ValidateLifetime(accessToken)) goto Abort;

            var claims = jwt.GetClaims(accessToken);
            var userID = claims.FirstOrDefault(x => x.Type == "UserID");
            if (userID is null) goto Abort;

            await Groups.AddToGroupAsync(Context.ConnectionId, userID.Value);
            return;

        Abort:
            Context.Abort();
            return;
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var context = Context.GetHttpContext();
            context!.Request.Headers.TryGetValue("AccessToken", out StringValues accessTokens);
            var accessToken = accessTokens[0];

            var claims = jwt.GetClaims(accessToken);
            var userID = claims.FirstOrDefault(x => x.Type == "UserID");
            if (userID is not null)                 
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userID!.Value);
        }
    }
}
