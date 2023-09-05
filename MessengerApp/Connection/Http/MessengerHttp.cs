using MessengerApp.Connection.Schemas;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace MessengerApp.Http
{

#nullable enable
    public partial class MessengerHttp
    {
        private readonly TokensContainer tokens;
        private HttpClientHandler httpHandler;
        private HttpClient client;

        public event Action? SignedIn;
        public event Action? SignedOut;

        public MessengerHttp(Uri address, TokensContainer tokens)
        {
            this.tokens = tokens;

            httpHandler = new HttpClientHandler();
            client = new(httpHandler);
            client.BaseAddress = address;
        }

        private static Uri GetUri(string url) => new(url, UriKind.Relative);

        private static HttpRequestMessage CreateRequest(HttpMethod method, string route, object? schema = null)
        {
            var request = new HttpRequestMessage(method, GetUri(route));
            if (schema is not null) request.Content = JsonContent.Create(schema);
            return request;
        }

        private async Task<HttpResponseMessage?> TrySendAsync(HttpRequestMessage message)
        {
            HttpResponseMessage? responseMessage = null;
            var copy = message.Clone();

        SendRequest:
            if (tokens.Valid) message.Headers.Add("AccessToken", tokens.AccessJWT);

            try
            {
                responseMessage = await client.SendAsync(message);
            }
            catch { }

            if (responseMessage is null) return responseMessage;
            if (responseMessage.StatusCode is not HttpStatusCode.Unauthorized) return responseMessage;

            var response = await responseMessage.Content.ReadAsStringAsync();
            if (response is not "TokenExpired") return responseMessage;

            var request = CreateRequest(HttpMethod.Get, "Auth/RefreshToken", tokens);
            var refreshResponce = await client.SendAsync(request);

            if (!refreshResponce.IsSuccessStatusCode) return refreshResponce;

            var tokensJson = await refreshResponce.Content.ReadAsStringAsync();
            if (!tokens.TryParseJson(tokensJson)) throw new Exception("Can't parse tokens");

            message = copy;
            goto SendRequest;
        }

        private async Task<Response> SendAndWrapAsync(HttpMethod method, string route, object? schema = null)
        {
            var request = CreateRequest(method, route, schema);
            var response = await TrySendAsync(request);
            return Response.Wrap(response);
        }

        private async Task<Response<T>> SendAndWrapJsonAsync<T>(HttpMethod method, string route, object? schema = null)
        {
            var request = CreateRequest(method, route, schema);
            var response = await TrySendAsync(request);
            return Response<T>.WrapJson(response);
        }

        private async Task<Response<Stream>> SendAndWrapStreamAsync(HttpMethod method, string route, object? schema = null)
        {
            var request = CreateRequest(method, route, schema);
            var response = await TrySendAsync(request);
            return Response<Stream>.WrapStream(response);
        }

        private async Task GetUserInfoAsync()
        {
            var userInfoRequest = await SendAndWrapJsonAsync<UserInfo>(HttpMethod.Get, "Users/GetUserInfo");
            if (userInfoRequest.Success) ConnectionHub.UserInfo = userInfoRequest.Data!;
            else App.LogOut();
        }
    }
}
