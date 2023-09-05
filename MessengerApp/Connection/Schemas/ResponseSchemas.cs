using System;

#nullable enable
namespace MessengerApp.Connection.Schemas
{
    public record Chat(string ID, ChatType Type, string Name, string[] MembersIDs, DateTime LastUpdate);

    public record SearchedUser(string ID, string Username);

    public record Message(string ID, string ChatID, string Data, MessageType Type, DateTime SendTime, DateTime UpdateTime,
        string SenderID, string SenderUsername, bool IsDeleted);

    public record UserInfo(string ID, string Username, string Mail);

    public record ImageUpdateSchema(string ID);

    public record ChatUpdateSchema(string ID);

    public record Tokens(string AccessJWT, string RefreshJWT);
}
