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

        public static string SettingsDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".rchia");

        public static string SettingsFilePath => Path.Combine(SettingsDirectory, "settings.json");

        public static string DefaultEndpointsFilePath => Path.Combine(SettingsDirectory, "endpoints.json");

        public static dynamic GetSettings()
        {
            try
            {
                using var reader = new StreamReader(SettingsFilePath);
                var json = reader.ReadToEnd();
                var settings = JsonConvert.DeserializeObject<dynamic>(json, new ExpandoObjectConverter());

                return settings ?? GetDefaultSettings();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);

                return GetDefaultSettings();
            }
        }

        private static dynamic GetDefaultSettings()
        {
            dynamic settings = new ExpandoObject();
            settings.endpointsfile = DefaultEndpointsFilePath;
            return settings;
        }

        private static void Initialize()
        {
            try
            {
                // create the settings folder and file if they don't exist
                if (!Directory.Exists(SettingsDirectory))
                {
                    _ = Directory.CreateDirectory(SettingsDirectory);
                }

                if (!File.Exists(SettingsFilePath))
                {
                    var defaultEndPointFile = Path.Combine(SettingsDirectory, SettingsFilePath);

                    var json = JsonConvert.SerializeObject(GetDefaultSettings(), Formatting.Indented, new ExpandoObjectConverter());

                    using var writer = new StreamWriter(SettingsFilePath, false);
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
