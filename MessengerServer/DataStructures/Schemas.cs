namespace MessengerServer.DataStructures
{
    public record SignInViaMailSchema(string Mail, string Password, string? DeviceInfo);

    public record SignUpViaMailSchema(string Username, string Mail, string Password);

    public record ConfirmAccountViaMailSchema(string Mail, string Code);

    public record SearchUsersByNameSchema(string SearchString, string? SkipWhileID, int Count);

    public record CreatePersonalChatSchema(string ContactID);

    public record RefreshTokenSchema(string AccessJWT, string RefreshJWT);

    public record GetChatsSchema(int Count, string? SkipWhileID);

    public record AddTextMessageSchema(string ChatID, string Text, DateTime SendTime);

    public record AddFileMessageSchema(string ChatID, DateTime SendTime, FileType FileType);

    public record EditTextMessageSchema(string MessageID, string Text);

    public record GetMessagesSchema(string ChatID, int Count, string? SkipWhileID);

    public record LogOutSchema(string AccessToken);

    public record GetChatImageSchema(string ChatID);

    public record GetUserProfileImageSchema(string UserID);

    public record DeleteMessageSchema(string MessageID);

    public record ChangeUsernameShema(string Username);

    public record ChangeMailShema(string Mail);

    public record ChangePasswordShema(string OldPassword, string NewPassword);    

    public record GetMessageFileSchema(string MessageID);
}
