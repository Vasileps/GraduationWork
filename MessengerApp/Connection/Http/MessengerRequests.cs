using MessengerApp.Connection.Schemas;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MessengerApp.Http
{
    public partial class MessengerHttp
    {
        public async Task<Response> SignUpViaMailAsync(SignUpViaMailSchema schema)
        {
            if (schema.Mail is null or "")
                return Response.Bad("EmailNullOrEmpty");
            if (schema.Password is null or "")
                return Response.Bad("PasswordNullOrEmpty");
            if (schema.Username is null or "")
                return Response.Bad("UsernameNullOrEmpty");

            return await SendAndWrapAsync(HttpMethod.Post, "Registration/SignUpViaMail", schema);
        }

        public async Task<Response> SignInViaMailAsync(SignInViaMailSchema schema)
        {
            if (schema.Mail is null or "")
                return Response.Bad("EmailNullOrEmpty");
            if (schema.Password is null or "")
                return Response.Bad("PasswordNullOrEmpty");

            var response = await SendAndWrapJsonAsync<Tokens>(HttpMethod.Get, "Auth/SignInViaMail", schema);
            if (response.Success)
            {
                tokens.SetTokens(response.Data!);
                await GetUserInfoAsync();
                SignedIn?.Invoke();
                return Response.Ok();
            }
            else return Response.Bad(response.ErrorMessage!);
        }

        public async Task<Response> ConfirmAccountViaMailAsync(ConfirmAccountViaMailSchema schema)
        {
            if (schema.Mail is null or "")
                return Response.Bad("MailNullOrEmpty");
            if (schema.Code is null or "")
                return Response.Bad("PasswordNullOrEmpty");

            return await SendAndWrapAsync(HttpMethod.Post, "Registration/ConfirmAccountViaMail", schema);
        }

        public async Task<Response<SearchedUser[]>> SearchUsersAsync(SearchUsersByNameSchema schema)
        {
            if (schema.Count <= 0)
                return Response<SearchedUser[]>.Bad("CountLessOrEqualsZero");
            if (string.IsNullOrEmpty(schema.SearchString))
                return Response<SearchedUser[]>.Bad("SearchStringNullOrEmpty");

            return await SendAndWrapJsonAsync<SearchedUser[]>(HttpMethod.Get, "Users/SearchUsers", schema);
        }

        public async Task<Response> CheckIfSignedInAsync()
        {
            var response = await SendAndWrapAsync(HttpMethod.Get, "Auth/CheckIfSignedIn");
            if (response.Success)
            {
                await GetUserInfoAsync();
                SignedIn?.Invoke();
            }
            return response;
        }

        public async Task<Response> CreatePersonalChatAsync(CreatePersonalChatSchema schema)
        {
            if (string.IsNullOrEmpty(schema.ContactID))
                return Response.Bad("ContactIDNullOrEmpty");

            return await SendAndWrapAsync(HttpMethod.Post, "Chats/CreatePersonalChat", schema);
        }

        public async Task<Response<Chat[]>> GetChatsAsync(GetChatsSchema schema)
        {
            if (schema.Count <= 0)
                return Response<Chat[]>.Bad("CountShouldBePositive");

            return await SendAndWrapJsonAsync<Chat[]>(HttpMethod.Get, "Chats/GetChats", schema);
        }

        public async Task<Response<Message[]>> GetMessagesDescendingAsync(GetMessagesSchema schema)
        {
            if (string.IsNullOrEmpty(schema.ChatID))
                return Response<Message[]>.Bad("ChatIDNullOrEmpty");
            if (schema.Count <= 0)
                return Response<Message[]>.Bad("CountShouldBePositive");

            return await SendAndWrapJsonAsync<Message[]>(HttpMethod.Get, "Messages/GetMessagesDescending", schema);
        }

        public async Task<Response<Message[]>> GetMessagesAscendingAsync(GetMessagesSchema schema)
        {
            if (string.IsNullOrEmpty(schema.ChatID))
                return Response<Message[]>.Bad("ChatIDNullOrEmpty");
            if (schema.Count <= 0)
                return Response<Message[]>.Bad("CountShouldBePositive");

            return await SendAndWrapJsonAsync<Message[]>(HttpMethod.Get, "Messages/GetMessagesAscending", schema);
        }

        public async Task<Response> AddTextMessageAsync(AddTextMessageSchema schema)
        {
            if (string.IsNullOrEmpty(schema.ChatID))
                return Response.Bad("ChatIDNullOrEmpty");
            if (string.IsNullOrEmpty(schema.Text))
                return Response.Bad("TextNullOrEmpty");

            return await SendAndWrapAsync(HttpMethod.Post, "Messages/AddTextMessage", schema);
        }

        public async Task<Response> SignOut()
        {
            if (!tokens.Valid) return Response.Ok();

            var response = await SendAndWrapAsync(HttpMethod.Get, "Auth/SignOut", new LogOutSchema(tokens.AccessJWT!));
            if (response.Success) SignedOut?.Invoke();
            return response;
        }

        public async Task<Response<BitmapImage>> GetChatImageAsync(GetChatImageSchema schema)
        {
            if (string.IsNullOrEmpty(schema.ChatID))
                return Response<BitmapImage>.Bad("ChatIDNullOrEmpty");

            var response = await SendAndWrapStreamAsync(HttpMethod.Get, "Files/GetChatImage", schema);
            if (!response.Success) return Response<BitmapImage>.Bad(response.ErrorMessage!);

            return Response<BitmapImage>.Ok(response.Data.ToBitmapImage());
        }

        public async Task<Response<BitmapImage>> GetUserProfileImageAsync(GetUserProfileImageSchema schema)
        {
            if (string.IsNullOrEmpty(schema.UserID))
                return Response<BitmapImage>.Bad("UserIDNullOrEmpty");

            var response = await SendAndWrapStreamAsync(HttpMethod.Get, "Files/GetUserProfileImage", schema);
            if (!response.Success) return Response<BitmapImage>.Bad(response.ErrorMessage!);

            return Response<BitmapImage>.Ok(response.Data.ToBitmapImage());
        }

        public async Task<Response> UpdateProfileImage(Stream imageStream)
        {
            var request = CreateRequest(HttpMethod.Post, "Files/UpdateProfileImage");

            using var form = new MultipartFormDataContent();
            form.Add(new StreamContent(imageStream), "Image", "Image");

            request.Content = form;

            var response = await TrySendAsync(request);
            return Response.Wrap(response);
        }

        public async Task<Response> DeleteMessage(DeleteMessageSchema schema)
        {
            if (string.IsNullOrEmpty(schema.MessageID))
                return Response.Bad("MessageIDNullOrEmpty");

            return await SendAndWrapAsync(HttpMethod.Post, "Messages/DeleteMessage", schema);
        }

        public async Task<Response> EditTextMessage(EditTextMessageSchema schema)
        {
            if (string.IsNullOrEmpty(schema.MessageID))
                return Response.Bad("MessageIDNullOrEmpty");
            if (string.IsNullOrEmpty(schema.Text))
                return Response.Bad("TextNullOrEmpty");

            return await SendAndWrapAsync(HttpMethod.Post, "Messages/EditTextMessage", schema);
        }

        public async Task<Response> ChangeUsernameAsync(ChangeUsernameShema schema)
        {
            if (string.IsNullOrEmpty(schema.Username))
                return Response.Bad("UsernameNullOrEmpty");

            return await SendAndWrapAsync(HttpMethod.Post, "Users/ChangeUsername", schema);
        }

        public async Task<Response> ChangePasswordAsync(ChangePasswordShema schema)
        {
            if (string.IsNullOrEmpty(schema.OldPassword))
                return Response.Bad("OldPasswordNullOrEmpty");
            if (string.IsNullOrEmpty(schema.NewPassword))
                return Response.Bad("NewPasswordNullOrEmpty");

            return await SendAndWrapAsync(HttpMethod.Post, "Users/ChangePassword", schema);
        }

        public async Task<Response> ChangeMailAsync(ChangeMailShema schema)
        {
            if (string.IsNullOrEmpty(schema.Mail)) return Response.Bad("MailNullOrEmpty");

            return await SendAndWrapAsync(HttpMethod.Post, "Users/ChangeMail", schema);
        }

        public async Task<Response<Stream>> GetMessageFile(GetMessageFileSchema schema)
        {
            if (string.IsNullOrEmpty(schema.MessageID)) return Response<Stream>.Bad("MessageIDNullOrEmpty");

            return await SendAndWrapStreamAsync(HttpMethod.Get, "Files/GetMessageFile", schema);
        }

        public async Task<Response> UpdateImageMessage(UpdateFileMessageSchema schema)
        {
            var request = CreateRequest(HttpMethod.Post, "Files/UpdateImageMessage");

            using var form = new MultipartFormDataContent();
            form.Add(new StreamContent(schema.File), "Image", schema.FileName);
            form.Add(new StringContent(schema.MessageID), "MessageID");

            request.Content = form;

            var response = await TrySendAsync(request);
            return Response.Wrap(response);
        }

        public async Task<Response> UpdateFileMessage(UpdateFileMessageSchema schema)
        {
            var request = CreateRequest(HttpMethod.Post, "Files/UpdateFileMessage");

            using var form = new MultipartFormDataContent();
            form.Add(new StreamContent(schema.File), "File", schema.FileName);
            form.Add(new StringContent(schema.MessageID), "MessageID");

            request.Content = form;

            var response = await TrySendAsync(request);
            return Response.Wrap(response);
        }

        public async Task<Response<Message>> AddFileMessageAsync(AddFileMessageSchema schema)
        {
            if (string.IsNullOrEmpty(schema.ChatID))
                return Response<Message>.Bad("ChatIDNullOrEmpty");

            return await SendAndWrapJsonAsync<Message>(HttpMethod.Post, "Messages/AddFileMessage", schema);
        }
    }
}
