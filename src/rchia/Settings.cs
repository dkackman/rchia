using System;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace rchia
{
    internal static class Settings
    {
        static Settings()
        {
            Initialize();
        }

        public static string ConfigDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".rchia");

        public static string ConfigFilePath => Path.Combine(ConfigDirectory, "settings.json");

        public static string DefaultEndpointsFilePath => Path.Combine(ConfigDirectory, "endpoints.json");

        public static dynamic GetConfig()
        {
            try
            {
                using var reader = new StreamReader(ConfigFilePath);
                var json = reader.ReadToEnd();
                var config = JsonConvert.DeserializeObject<dynamic>(json, new ExpandoObjectConverter());

                return config ?? GetDefaultConfig();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);

                return GetDefaultConfig();
            }
        }

        private static dynamic GetDefaultConfig()
        {
            dynamic config = new ExpandoObject();
            config.endpointsfile = DefaultEndpointsFilePath;
            return config;
        }

        private static void Initialize()
        {
            try
            {
                // create the settings folder and file if they don't exist
                if (!Directory.Exists(ConfigDirectory))
                {
                    _ = Directory.CreateDirectory(ConfigDirectory);
                }

                if (!File.Exists(ConfigFilePath))
                {
                    var defaultEndPointFile = Path.Combine(ConfigDirectory, ConfigFilePath);

                    var json = JsonConvert.SerializeObject(GetDefaultConfig(), Formatting.Indented, new ExpandoObjectConverter());

                    using var writer = new StreamWriter(ConfigFilePath, false);
                    writer.WriteLine(json);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
    }
}
