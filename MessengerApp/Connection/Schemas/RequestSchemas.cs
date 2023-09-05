#nullable enable
using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;

namespace MessengerApp.Connection.Schemas
{
    public record SignInViaMailSchema(string Mail, string Password);

    public record SignUpViaMailSchema(string Username, string Mail, string Password);

    public record ConfirmAccountViaMailSchema(string Mail, string Code);

    public record SearchUsersByNameSchema(string SearchString, string? SkipWhileID, int Count);

    public record CreatePersonalChatSchema(string ContactID);

    public record GetChatsSchema(int Count, string? SkipWhileID);

    public record GetMessagesSchema(string ChatID, int Count, string? SkipWhileID);

    public record AddTextMessageSchema(string ChatID, string Text, DateTime SendTime);

    public record LogOutSchema(string AccessToken);

    public record GetChatImageSchema(string ChatID);

    public record GetUserProfileImageSchema(string UserID);

    public record DeleteMessageSchema(string MessageID);

    public record EditTextMessageSchema(string MessageID, string Text);

    public record ChangeUsernameShema(string Username);

    public record ChangeMailShema(string Mail);

    public record ChangePasswordShema(string OldPassword, string NewPassword);

    public record GetMessageFileSchema(string MessageID);

    public record UpdateFileMessageSchema(Stream File, string FileName, string MessageID);

    public record AddFileMessageSchema(string ChatID, DateTime SendTime, FileType FileType);
}
