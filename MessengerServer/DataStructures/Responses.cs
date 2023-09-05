namespace MessengerServer.DataStructures
{
    public record Tokens(string AccessJWT, string RefreshJWT);

    public record ProfileInfo(string ID, string Username, string Mail);
}
