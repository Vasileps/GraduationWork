using System.IO;
using System.Text.Json;

namespace MessengerApp.Http
{
#nullable enable
    public class Response
    {
        public string? ErrorMessage { get; private set; }
        public bool Success { get; private set; }

        private Response() { }

        public static Response Bad(string message)
        {
            var result = new Response()
            {
                ErrorMessage = message,
                Success = false,
            };

            return result;
        }

        public static Response Ok()
        {
            var result = new Response()
            {
                Success = true,
            };

            return result;
        }

        public static Response Wrap(HttpResponseMessage? message)
        {
            if (message is null) return Bad("ConnectionError");

            if (message.IsSuccessStatusCode) return Ok();

            var reading = message.Content.ReadAsStringAsync();
            reading.Wait();

            return Bad(reading.Result!);
        }
    }

#nullable enable
    public class Response<T>
    {
        public string? ErrorMessage { get; private set; }
        public bool Success { get; private set; }
        public T? Data { get; private set; }

        private Response() { }

        public static Response<T> Bad(string message)
        {
            var result = new Response<T>()
            {
                ErrorMessage = message,
                Success = false,
            };

            return result;
        }

        public static Response<T> Ok(T obj)
        {
            var result = new Response<T>()
            {
                Success = true,
                Data = obj,
            };

            return result;
        }

        public static Response<T> WrapJson(HttpResponseMessage? message)
        {
            if (message is null) return Bad("ConnectionError");

            var reading = message.Content.ReadAsStringAsync();
            reading.Wait();

            string content = reading.Result!;

            if (!message.IsSuccessStatusCode) return Bad(content);

            var options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            };

            var data = JsonSerializer.Deserialize<T>(content, options)!;

            return Ok(data);
        }

        public static Response<Stream> WrapStream(HttpResponseMessage? message)
        {
            if (message is null) return Response<Stream>.Bad("ConnectionError");

            if (!message.IsSuccessStatusCode)
            {
                var readingString = message.Content.ReadAsStringAsync();
                readingString.Wait();
                return Response<Stream>.Bad(readingString.Result!);
            }

            var readingStream = message.Content.ReadAsStreamAsync();
            readingStream.Wait();

            return Response<Stream>.Ok(readingStream.Result);
        }
    }
}
