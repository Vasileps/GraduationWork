using MessengerServer.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MessengerServer.Services
{
    public class JWTService : IJWTService
    {
        private string issuer;
        private string audience;
        private SymmetricSecurityKey key;
        private TokenValidationParameters claimsParams;

        public JWTService(IConfiguration configuration)
        {
            issuer = configuration.GetValue<string>("Jwt:Issuer")!;
            audience = configuration.GetValue<string>("Jwt:Audience")!;

            string stringKey = configuration.GetValue<string>("Jwt:Key")!;
            key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(stringKey));
            if (key.KeySize < 32) throw new ArgumentException("JwtKey is too short! It shoud be at list 32bit long");

            claimsParams = new TokenValidationParameters
            {
                ValidateLifetime = false,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidAudience = audience,
                ValidIssuer = issuer,
            };
        }

        public string GetToken(List<Claim> claims, TimeSpan? lifeTime = null)
        {
            DateTime? expires = null;
            if (lifeTime is TimeSpan time)
                expires = DateTime.UtcNow.Add(time);

            var jwt = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        public bool ValidateLifetime(string token)
        {
            var claims = GetClaims(token);
            var timeClaim = claims.FirstOrDefault(x => x.Type == "exp");
            if (timeClaim is null) return false;

            if (!long.TryParse(timeClaim.Value, out long seconds)) return false;
            var dateTime = DateTimeOffset.FromUnixTimeSeconds(seconds);
            return dateTime > DateTime.UtcNow;
        }

        public IEnumerable<Claim> GetClaims(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token, claimsParams, out SecurityToken validatedToken);
                return ((JwtSecurityToken)validatedToken).Claims;
            }
            catch
            {
                return Enumerable.Empty<Claim>();
            }
        }
    }
}
