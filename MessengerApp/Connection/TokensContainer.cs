using MessengerApp.Connection.Schemas;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MessengerApp.Connection
{
#nullable enable
    public class TokensContainer
    {
        public event Action? Updated;

        public string? AccessJWT { get; private set; }

        public string? RefreshJWT { get; private set; }

        [JsonIgnore]
        public bool Valid { get; private set; } = false;

        public bool TryParseJson(string json)
        {
            Tokens? tokens;
            var options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            try
            {
                tokens = JsonSerializer.Deserialize<Tokens>(json, options);
                if (tokens is null) return Valid = false;
            }
            catch
            {
                return Valid = false;
            }

            AccessJWT = tokens.AccessJWT;
            RefreshJWT = tokens.RefreshJWT;
            Updated?.Invoke();
            return Valid = true;
        }

        public void SetTokens(Tokens schema)
        {
            if (string.IsNullOrEmpty(schema.AccessJWT)) throw new ArgumentException(nameof(schema.AccessJWT));
            if (string.IsNullOrEmpty(schema.RefreshJWT)) throw new ArgumentException(nameof(schema.RefreshJWT));

            AccessJWT = schema.AccessJWT;
            RefreshJWT = schema.RefreshJWT;
            Valid = true;
            Updated?.Invoke();
        }

        public void Clear()
        {
            AccessJWT = null;
            RefreshJWT = null;
            Valid = false;
            Updated?.Invoke();
        }
    }
}
