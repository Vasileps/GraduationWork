using MessengerServer.Database.Interfaces;
using MessengerServer.DataStructures;
using MessengerServer.Services.Interfaces;

namespace MessengerServer.Controllers
{
    [ApiController]
    [Route(nameof(Users))]
    public class Users : Controller
    {
        private readonly IUserRepository userRep;
        private readonly IChatRepository chatRep;
        private readonly IMessageRepository messageRep;
        private readonly IContactListRepository contactListRep;
        private readonly IChatListRepository chatListRep;
        private readonly IConfiguration configuration;
        private readonly INotificationService notifications;

        public Users(IUserRepository userRepository, IChatRepository chatRepository, IChatListRepository chatListRepository,
            IMessageRepository messageRepository, IConfiguration configuration, INotificationService notificationService,
            IContactListRepository contactListRep)
        {
            this.userRep = userRepository;
            this.chatRep = chatRepository;
            this.configuration = configuration;
            this.notifications = notificationService;
            this.contactListRep = contactListRep;
            this.chatListRep = chatListRepository;
            this.messageRep = messageRepository;
        }

        [HttpGet]
        [Route(nameof(GetUserInfo))]
        [AuthRequired]
        public async Task<ActionResult> GetUserInfo()
        {
            var userID = (string)HttpContext.Items["UserID"]!;
            var user = await userRep.GetByIDAsync(userID);
            if (user is null) return StatusCode(500);

            return Json(new ProfileInfo(user.ID, user.Username, user.Mail));
        }

#nullable enable
        [HttpGet]
        [Route(nameof(SearchUsers))]
        [AuthRequired]
        public async Task<ActionResult> SearchUsers([FromBody] SearchUsersByNameSchema schema)
        {
            if (schema.Count <= 0) return BadRequest("CountLessOrEqualsZero");
            if (string.IsNullOrEmpty(schema.SearchString)) return BadRequest("SearchStringNullOrEmpty");

            var userID = (string)HttpContext.Items["UserID"]!;

            var users = await userRep.SearchByUsernameAsync(schema.SearchString, schema.Count, schema.SkipWhileID, userID);

            return Json(users.ToArray());
        }

        [HttpPost]
        [Route(nameof(ChangeMail))]
        [AuthRequired]
        public async Task<ActionResult> ChangeMail(ChangeMailShema schema)
        {
            if (string.IsNullOrEmpty(schema.Mail)) return BadRequest("MailNullOrEmpty");

            var check = await userRep.GetByMailAsync(schema.Mail);
            if (check is not null) return BadRequest("MailTaken");

            var userID = (string)HttpContext.Items["UserID"]!;
            var user = await userRep.GetByIDAsync(userID);
            if (user is null) return StatusCode(500);

            user.Mail = schema.Mail;
            await userRep.UpdateAsync(user);

            await notifications.ProfileUpdated(new(user.ID, user.Username, user.Mail));

            return Ok();
        }

        [HttpPost]
        [Route(nameof(ConfirmMailChange))]
        [AuthRequired]
        public ActionResult ConfirmMailChange()
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Route(nameof(ChangePassword))]
        [AuthRequired]
        public async Task<ActionResult> ChangePassword(ChangePasswordShema schema)
        {
            if (string.IsNullOrEmpty(schema.NewPassword))
                return BadRequest("NewPasswordNullOrEmpty");
            if (string.IsNullOrEmpty(schema.OldPassword))
                return BadRequest("OldPasswordNullOrEmpty");

            var userID = (string)HttpContext.Items["UserID"]!;
            var user = await userRep.GetByIDAsync(userID);
            if (user is null) return StatusCode(500);

            if (!user.ComparePassword(schema.OldPassword)) return BadRequest("WrongPassword");

            user.UpdatePassword(schema.NewPassword);
            await userRep.UpdateAsync(user);

            return Ok();
        }

        [HttpGet]
        [Route(nameof(GetContacts))]
        [AuthRequired]
        public async Task<ActionResult> GetContacts()
        {
            throw new NotImplementedException();
        }


        [HttpPost]
        [Route(nameof(ChangeUsername))]
        [AuthRequired]
        public async Task<ActionResult> ChangeUsername(ChangeUsernameShema schema)
        {
            if (string.IsNullOrEmpty(schema.Username))
                return BadRequest("UsernameNullOrEmpty");

            var check = await userRep.GetByUsernameAsync(schema.Username);
            if (check is not null) return BadRequest("UsernameTaken");

            var userID = (string)HttpContext.Items["UserID"]!;
            var user = await userRep.GetByIDAsync(userID);
            if (user is null) return StatusCode(500);

            user.Username = schema.Username;
            await userRep.UpdateAsync(user);

            await messageRep.UpdateSenderUsernameAsync(user.ID, user.Username);

            var tasks = new List<Task>();
            var chatList = await chatListRep.GetByUserIDAsync(userID);
            if (chatList is null) chatList = await chatListRep.AddAsync(new(userID));

            var chats = await chatRep.GetManyByIDsAsync(chatList.Chats);

            tasks.Add(notifications.ProfileUpdated(new(user.ID, user.Username, user.Mail)));
            foreach (var chat in chats.Where(x => x.Type == ChatType.Personal))
                tasks.Add(notifications.ChatInfoUpdated(chat));

            await Task.WhenAll(tasks);

            return Ok();
        }
    }
}
