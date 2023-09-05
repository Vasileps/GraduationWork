using MessengerServer.Database.Interfaces;
using MessengerServer.DataStructures;
using MessengerServer.Services.Interfaces;
using MongoDB.Driver;

namespace MessengerServer.Controllers
{
    [ApiController]
    [Route(nameof(Files))]
    public class Files : Controller
    {
        private const double maxImageSize = 1024;

        private readonly IContactListRepository contactsRep;
        private readonly IChatRepository chatRep;
        private readonly IFileMetadataRepository metadataRep;
        private readonly INotificationService notifications;
        private readonly IFilesProvider filesProvider;
        private readonly IMessageRepository messageRep;

        public Files(IFileMetadataRepository metadataRepository, IChatRepository chatRepository, IMessageRepository messageRepository,
            IContactListRepository contactListRepository, INotificationService notificationService, IFilesProvider filesProvider)
        {
            this.chatRep = chatRepository;
            this.contactsRep = contactListRepository;
            this.metadataRep = metadataRepository;
            this.notifications = notificationService;
            this.filesProvider = filesProvider;
            this.messageRep = messageRepository;
        }

        [HttpPost]
        [Route(nameof(UpdateProfileImage))]
        [AuthRequired]
        public async Task<ActionResult> UpdateProfileImage(IFormCollection content)
        {
            var imageStream = content.Files["Image"];
            if (imageStream is null) return BadRequest("ImageNull");

            var image = Image.Load(imageStream.OpenReadStream());
            image.Mutate(x => x.Resize(256, 256));

            var userID = (string)HttpContext.Items["UserID"]!;
            var relativePath = Path.Combine("UserImages", userID);

            var stream = new MemoryStream();
            image.SaveAsJpeg(stream);
            stream.Flush();
            image.Dispose();

            await filesProvider.UpdateFileAsync(stream, relativePath);

            var metadata = await metadataRep.GetByPathAsync(relativePath);

            if (metadata is null) await metadataRep.AddAsync(new(relativePath, $"{userID}.jpg", stream.Length));
            else
            {
                metadata.UpdateTime = DateTime.UtcNow;
                metadata.FileName = $"{userID}.jpg";
                metadata.Size = stream.Length;
                await metadataRep.UpdateAsync(metadata);
            }

            stream.Dispose();
            await notifications.UserImageUpdated(userID);

            return Ok();
        }

        [HttpPost]
        [Route(nameof(UpdateImageMessage))]
        [AuthRequired]
        public async Task<ActionResult> UpdateImageMessage(IFormCollection content)
        {
            var imageFile = content.Files["Image"];
            if (imageFile is null) return BadRequest("ImageIsNull");

            var fileName = imageFile.FileName;
            var image = Image.Load(imageFile.OpenReadStream());

            var largeSideSize = Math.Max(image.Width, image.Height);
            var scaleFactor = largeSideSize / maxImageSize;
            if (scaleFactor > 1)
            {
                int width = (int)(image.Width / scaleFactor);
                int height = (int)(image.Height / scaleFactor);
                image.Mutate(x => x.Resize(width, height));
            }

            var userID = (string)HttpContext.Items["UserID"]!;
            if (!content.TryGetValue("MessageID", out var messageID)) return BadRequest("MessageIDNull");

            var message = await messageRep.GetByIDAsync(messageID[0]);
            if (message is null) return BadRequest("MessageNotFound");
            if (message.SenderID != userID) return BadRequest("CantEditAnothersMessage");

            var relativePath = Path.Combine("Chats", message.ChatID, message.ID);

            var stream = new MemoryStream();
            image.SaveAsJpeg(stream);
            stream.Flush();
            stream.Position = 0;
            image.Dispose();

            await filesProvider.UpdateFileAsync(stream, relativePath);

            var metadata = await metadataRep.GetByPathAsync(relativePath);
            if (metadata is null) await metadataRep.AddAsync(new(relativePath, fileName, stream.Length));
            else
            {
                metadata.UpdateTime = DateTime.UtcNow;
                metadata.FileName = fileName;
                metadata.Size = stream.Length;
                await metadataRep.UpdateAsync(metadata);
            }

            stream.Dispose();
            var chat = await chatRep.GetByIDAsync(message.ChatID);

            await notifications.MessageUpdated(message, chat?.MembersIDs ?? new string[0]);

            return Ok();
        }

        [HttpPost]
        [Route(nameof(UpdateFileMessage))]
        [AuthRequired]
        public async Task<ActionResult> UpdateFileMessage(IFormCollection content)
        {
            var fileCollection = content.Files["File"];
            if (fileCollection is null) return BadRequest("FileNull");

            var fileName = fileCollection.FileName;
            var stream = fileCollection.OpenReadStream();

            var userID = (string)HttpContext.Items["UserID"]!;
            if (!content.TryGetValue("MessageID", out var messageID)) return BadRequest("MessageIDNull");

            var message = await messageRep.GetByIDAsync(messageID[0]);
            if (message is null) return BadRequest("MessageNotFound");
            if (message.SenderID != userID) return BadRequest("CantEditAnothersMessage");

            if (stream.Length > GlobalValues.MaxFileSize)
            {
                if (string.IsNullOrEmpty(message.Data))
                {
                    message.IsDeleted = true;
                    await messageRep.UpdateAsync(message);
                    await notifications.MessageDeleted(message);
                }
                return BadRequest("FileTooBig");
            }

            var relativePath = Path.Combine("Chats", message.ChatID, message.ID);

            await filesProvider.UpdateFileAsync(stream, relativePath);
            var metadata = await metadataRep.GetByPathAsync(relativePath);

            if (metadata is null) metadata = await metadataRep.AddAsync(new(relativePath, fileName, stream.Length));
            else
            {
                metadata.UpdateTime = DateTime.UtcNow;
                metadata.FileName = fileName;
                metadata.Size = stream.Length;
                await metadataRep.UpdateAsync(metadata);
            }
            stream.Dispose();

            message.SetFileMetadata(metadata);
            await messageRep.UpdateAsync(message);

            var chat = await chatRep.GetByIDAsync(message.ChatID);
            await notifications.MessageUpdated(message, chat?.MembersIDs ?? new string[0]);

            return Ok();
        }

        [HttpGet]
        [Route(nameof(GetUserProfileImage))]
        [AuthRequired]
        public async Task<ActionResult> GetUserProfileImage([FromBody] GetUserProfileImageSchema schema)
        {
            if (string.IsNullOrEmpty(schema.UserID)) return BadRequest("UserIDNullOrEmpty");

            var relativePath = Path.Combine("UserImages", schema.UserID);

            var metadata = await metadataRep.GetByPathAsync(relativePath);
            var stream = await filesProvider.GetFileAsync(relativePath);

            if (stream is null) return BadRequest("UserDoesntHaveImage");

            return File(stream, "application/octet-stream", metadata?.FileName);
        }

        [HttpGet]
        [Route(nameof(GetMessageFile))]
        [AuthRequired]
        public async Task<ActionResult> GetMessageFile([FromBody] GetMessageFileSchema schema)
        {
            if (string.IsNullOrEmpty(schema.MessageID)) return BadRequest("MessageIDNullOrEmpty");

            var message = await messageRep.GetByIDAsync(schema.MessageID);
            if (message is null) return BadRequest("MessageNotFound");

            var chat = await chatRep.GetByIDAsync(message.ChatID);
            if (chat is null) return StatusCode(500);

            var userID = (string)HttpContext.Items["UserID"]!;
            if (!chat.MembersIDs.Contains(userID)) return BadRequest("NotChatMember");

            var relativePath = Path.Combine("Chats", message.ChatID, message.ID);

            var stream = await filesProvider.GetFileAsync(relativePath);
            if (stream is null) return NotFound("ImageNotFound");

            var metadata = await metadataRep.GetByPathAsync(relativePath);
            if (metadata is null) metadata = await metadataRep.AddAsync(new(relativePath, "File", stream.Length));

            return File(stream, "application/octet-stream", metadata.FileName);
        }

        [HttpGet]
        [Route(nameof(GetChatImage))]
        [AuthRequired]
        public async Task<ActionResult> GetChatImage([FromBody] GetChatImageSchema schema)
        {
            if (string.IsNullOrEmpty(schema.ChatID)) return BadRequest("ChatIDNullOrEmpty");

            var chat = await chatRep.GetByIDAsync(schema.ChatID);
            if (chat is null) return BadRequest("ChatDoesntExist");

            var userID = (string)HttpContext.Items["UserID"]!;
            if (!chat.MembersIDs.Contains(userID)) return BadRequest("UserNotMemberOfChat");


            if (chat.Type == ChatType.Personal)
            {
                var contactID = chat.MembersIDs.Single(x => x != userID);
                var relativePath = Path.Combine("UserImages", contactID);

                var metadata = await metadataRep.GetByPathAsync(relativePath);
                var stream = await filesProvider.GetFileAsync(relativePath);

                if (stream is null) return BadRequest("UserDoesntHaveImage");
                return File(stream, "application/octet-stream", metadata?.FileName);
            }

            return StatusCode(500);
        }
    }
}
