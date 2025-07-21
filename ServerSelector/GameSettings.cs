using System;
using System.IO;
using System.Text.Json;

namespace ServerSelector
{
    public class GameSettings
    {
        private static GameSettings? _settings;
        public string GameRoot { get; set; } = "C:\\Nikke";
        public string LastIp { get; set; } = "127.0.0.1";
        public int LastOffset { get; set; }

        public static GameSettings Settings
        {
            get
            {
                if (_settings != null)
                    return _settings;

                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "serverselectorsettings.json");

                try
                {
                    if (File.Exists(path))
                    {
                        string json = File.ReadAllText(path);
                        _settings = JsonSerializer.Deserialize<GameSettings>(json);
                    }
                }
                catch
                {

                }

                _settings ??= new();

                return _settings;
            }
        }

        public static void Save()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "serverselectorsettings.json");
            File.WriteAllText(path, JsonSerializer.Serialize(_settings));
        }
    }
}