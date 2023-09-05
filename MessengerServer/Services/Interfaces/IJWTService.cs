using System.Security.Claims;

namespace MessengerServer.Services.Interfaces
{
    public interface IJWTService
    {
        public bool ValidateLifetime(string token);

        public IEnumerable<Claim> GetClaims(string token);

        public string GetToken(List<Claim> claims, TimeSpan? lifeTime = null);
    }
}
