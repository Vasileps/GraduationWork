using MessengerServer.Database.Interfaces;
using MessengerServer.DataStructures;
using MessengerServer.Services.Interfaces;
using MongoDB.Driver;
using System.Security.Claims;

namespace MessengerServer.Controllers
{
    [ApiController]
    [Route(nameof(Auth))]
    public class Auth : Controller
    {
        private readonly ISessionRepository sessionRep;
        private readonly IUserRepository userRep;
        private readonly IJWTService jwt;

        public Auth(ISessionRepository sessionRepository, IUserRepository userRepository, IJWTService jwt)
        {
            this.sessionRep = sessionRepository;
            this.userRep = userRepository;
            this.jwt = jwt;
        }

        [HttpGet]
        [Route(nameof(SignInViaMail))]
        public async Task<ActionResult> SignInViaMail([FromBody] SignInViaMailSchema schema)
        {
            if (string.IsNullOrEmpty(schema.Mail)) return BadRequest("MailNullOrEmpty");
            if (string.IsNullOrEmpty(schema.Password)) return BadRequest("PasswordNullOrEmpty");

            var user = await userRep.GetByMailAsync(schema.Mail);

            if (user is null) return BadRequest("UserNotFound");
            if (!user.Verified) return BadRequest("UserNotVerified");
            if (!user.ComparePassword(schema.Password)) return BadRequest("WrongPassword");

            var tokens = await CreateTokens();

            var deviceInfo = schema.DeviceInfo;
            deviceInfo ??= HttpContext.Connection.RemoteIpAddress?.ToString();

            await sessionRep.AddAsync(new Session(tokens.Access, tokens.Refresh, user.ID, deviceInfo!));

            var JWTs = CreateJWT(tokens, user.ID);

            return Json(new Tokens(JWTs.Access, JWTs.Refresh));
        }

        [HttpGet]
        [Route(nameof(RefreshToken))]
        public async Task<ActionResult> RefreshToken([FromBody] RefreshTokenSchema schema)
        {
            if (string.IsNullOrEmpty(schema.AccessJWT))
                return BadRequest("AccessTokenNullOrEmpty");
            if (string.IsNullOrEmpty(schema.RefreshJWT))
                return BadRequest("RefreshTokenNullOrEmpty");

            if (!jwt.ValidateLifetime(schema.RefreshJWT)) return BadRequest("RefreshTokenExpired");

            var accessTokenID = jwt
                .GetClaims(schema.AccessJWT)?
                .FirstOrDefault(x=>x.Type == "Token")?
                .Value;
            var refreshTokenID = jwt
                .GetClaims(schema.RefreshJWT)?
                .FirstOrDefault(x => x.Type == "Token")?
                .Value;

            if (accessTokenID is null || refreshTokenID is null) return BadRequest("InvalidTokens");

            var session = await sessionRep.GetByAccessTokenAsync(accessTokenID);
            if (session is null) return StatusCode(500);

            if (session.RefreshToken != refreshTokenID) return BadRequest("InvalidRefreshToken");

            var tokens = await CreateTokens();

            session.AccessToken = tokens.Access;
            session.RefreshToken = tokens.Refresh;
            session.LastRefresh = DateTime.UtcNow;

            await sessionRep.UpdateAsync(session);

            var JWTs = CreateJWT(tokens, session.UserID);

            return Json(new Tokens(JWTs.Access, JWTs.Refresh));
        }

        [HttpGet]
        [Route(nameof(CheckIfSignedIn))]
        [AuthRequired]
        public ActionResult CheckIfSignedIn() => Ok();

        [HttpGet]
        [Route(nameof(SignOut))]
        [AuthRequired]
        public async Task<ActionResult> SignOut(LogOutSchema schema)
        {
            if (string.IsNullOrEmpty(schema.AccessToken))
                return BadRequest("AccessTokenNullOrEmpty");

            var session = await sessionRep.GetByAccessTokenAsync(schema.AccessToken);
            if (session is null) return Ok();

            await sessionRep.DeleteAsync(session.ID);

            return Ok();
        }

        [NonAction]
        private (string Access, string Refresh) CreateJWT((string Access, string Refresh) tokens, string userID)
        {
            var tokenClaims = new List<Claim>
            {
                new Claim("UserID", userID.ToString()),
                new Claim("Token", tokens.Access)
            };
            var refreshClaims = new List<Claim>
            {
                new Claim("Token", tokens.Refresh),
            };

            var accessJWT = jwt.GetToken(tokenClaims, GlobalValues.AccessTokenLifetime);
            var refreshJWT = jwt.GetToken(refreshClaims, GlobalValues.RefreshTokenLifetime);

            return (accessJWT, refreshJWT);
        }

        [NonAction]
        private async Task<(string Access, string Refresh)> CreateTokens()
        {
            string accessToken, refreshToken;
            do
            {
                refreshToken = Guid.NewGuid().ToString();
                accessToken = Guid.NewGuid().ToString();
            }
            while (await sessionRep.CheckIfTokenTakenAsync(refreshToken, accessToken));

            return (accessToken, refreshToken);
        }
    }
}
