using Dtwo.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtwo.App.DesktopApp.Configuration
{
    public class Configuration
    {
        public bool IsDebug { get; set; } = false;
        public bool LogToFile { get; set; } = false;

        public static string Path => System.IO.Path.Combine(Paths.ConfigDirectoryPath, "config.json");

        private static Configuration m_instance;
        public static Configuration Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = LoadConfiguration();
                }
                return m_instance;
            }
        }

        private static Configuration LoadConfiguration()
        {
            Configuration? configuration;

            if (Directory.Exists(System.IO.Path.GetDirectoryName(Path)) == false)
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
            }

            if (System.IO.File.Exists(Path) == false)
            {
                configuration = new Configuration();
                SaveConfiguration(configuration);
            }
            else
            {
                string json = System.IO.File.ReadAllText(Path);
                configuration = Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration>(json);

                if (configuration == null)
                {
                    LogManager.LogError("Configuration file is corrupted");
                    configuration = new Configuration();
                }
            }

            return configuration;

        }

        private static void SaveConfiguration(Configuration configuration)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(configuration);
            System.IO.File.WriteAllText(Path, json);
        }

        public void Save()
        {
            SaveConfiguration(this);
        }

        public Configuration Clone()
        {
            return new Configuration
            {
                IsDebug = IsDebug,
                LogToFile = LogToFile
            };
        }

        public static void UpdateConfiguration(Configuration configuration)
        {
            m_instance = configuration;
            SaveConfiguration(configuration);
        }
    }
}
