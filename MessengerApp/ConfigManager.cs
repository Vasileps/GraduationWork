using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MessengerApp
{
    public static class ConfigManager
    {
        private const string fileName = "config.json";

        private static Dictionary<string, string> config { get; set; }

        static ConfigManager()
        {
            var path = Path.Combine(App.DataDirectory.FullName, fileName);
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);

                try
                {
                    config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                }
                catch
                {
                    File.Delete(path);
                    config = new();
                }
            }
            else config = new();
        }

        public static void Save()
        {
            var path = Path.Combine(App.DataDirectory.FullName, fileName);

            var options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(path, json);
        }

#nullable enable
        public static bool TryGet(string key, out string? value)
        {
            value = null;
            if (config.ContainsKey(key))
            {
                value = config[key];
                return true;
            }
            else return false;
        }

        public static void Set(string key, string value)
        {
            if (config.ContainsKey(key)) config[key] = value;
            else config.Add(key, value);
        }
    }
}
