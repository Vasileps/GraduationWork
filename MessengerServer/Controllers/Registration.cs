using MessengerServer.Database.Interfaces;
using MessengerServer.DataStructures;
using MessengerServer.Services.Interfaces;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace MessengerServer.Controllers
{
    [ApiController]
    [Route(nameof(Registration))]
    public class Registration : Controller
    {
        private readonly IUserRepository userRep;
        private readonly IConfirmCodeRepository codesRep;
        private readonly IChatListRepository chatListRep;
        private readonly IContactListRepository contactListRep;
        private readonly IMailService mailService;

        public Registration(IUserRepository userRepository, IConfirmCodeRepository confirmCodeRepository, IChatListRepository chatListRepository,
            IContactListRepository contactListRepository, IMailService mailService)
        {
            this.userRep = userRepository;
            this.codesRep = confirmCodeRepository;
            this.mailService = mailService;
            this.chatListRep = chatListRepository;
            this.contactListRep = contactListRepository;
        }

        [HttpPost]
        [Route(nameof(SignUpViaMail))]
        public async Task<IActionResult> SignUpViaMail([FromBody] SignUpViaMailSchema schema)
        {
            if (string.IsNullOrEmpty(schema.Username)) return BadRequest("UsernameNullOrEmpty");
            if (string.IsNullOrEmpty(schema.Mail)) return BadRequest("MailNullOrEmpty");
            if (!Regex.IsMatch(schema.Mail, GlobalValues.MailRegex)) return BadRequest("BadMail");
            if (string.IsNullOrEmpty(schema.Password)) return BadRequest("PasswordNullOrEmpty");

            var checkMail = await userRep.GetByMailAsync(schema.Mail);
            if (checkMail is not null) return BadRequest("MailTaken");

            var checkUsername = await userRep.GetByUsernameAsync(schema.Username);
            if (checkUsername is not null) return BadRequest("UsernameTaken");

            var user = await userRep.AddAsync(new(schema.Username, schema.Mail, schema.Password));
            await chatListRep.AddAsync(new(user.ID));
            await contactListRep.AddAsync(new(user.ID));

            string code = Random.Shared.Next(100000, 1000000).ToString();
            await mailService.SendTextAsync(schema.Username, schema.Mail, $"Yore code is {code}");

            await codesRep.AddAsync(new(schema.Mail, code, ConfirmType.ConfirmAccountByMail));

            return Ok();
        }

        [HttpPost]
        [Route(nameof(ConfirmAccountViaMail))]
        public async Task<IActionResult> ConfirmAccountViaMail([FromBody] ConfirmAccountViaMailSchema schema)
        {
            if (string.IsNullOrEmpty(schema.Mail)) return BadRequest("MailNullOrEmpty");
            if (!Regex.IsMatch(schema.Mail, GlobalValues.MailRegex)) return BadRequest("BadMail");
            if (string.IsNullOrEmpty(schema.Code)) return BadRequest("CodeNullOrEmpty");

            var allCodes = await codesRep.GetBySourceAsync(schema.Mail);
            var matchingCodes = allCodes.Where(x => x.Type == ConfirmType.ConfirmAccountByMail && x.Code == schema.Code).ToArray();

            if (!matchingCodes.Any()) return BadRequest("WrongCode");

            var user = await userRep.GetByMailAsync(schema.Mail);
            if (user is null)
            {
                DeleteCodes(matchingCodes);
                return StatusCode(500);
            }

            user.Verified = true;
            await userRep.UpdateAsync(user);
            DeleteCodes(matchingCodes);

            return Ok();

            async void DeleteCodes(IEnumerable<ConfirmCode> codes)
            {
                foreach (var code in codes)
                    await codesRep.DeleteAsync(code.ID);
            }
        }
    }
}
