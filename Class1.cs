using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Collections;




namespace MiFirstService
{

    public class Settings
    {

        public string ServerAdress { get; set; }
        public string Port { get; set; }
        public string SKey { get; set; }
        public string Token { get; set; }
        public string InternalAdminPass { get; set; }

    }
    public class SKeyResponse
    {
        public string register { get; set; }
        public string secret_key { get; set; }
    }
    public class ConfigJson
    {
        Logging log = new Logging();

        
        static string PathToConfigFile = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Conf.json");
        bool FileExist = File.Exists(PathToConfigFile);
        public ConfigJson()
        {

            if (!FileExist) // if settings file not exist create with default values
            {

                Settings settings = new Settings();
                settings.Port = "9090";
                settings.ServerAdress = "localhost";
                File.WriteAllText(PathToConfigFile, JsonConvert.SerializeObject(settings));

            }
        }

        public string GetConfigFilePath()
        {
            return PathToConfigFile;
        }

        public object GetSettings() //берем настройки из файла 
        {
            try
            {
                StreamReader file = File.OpenText(PathToConfigFile);
                JsonSerializer serializer = new JsonSerializer();
                Settings settings = (Settings)serializer.Deserialize(file, typeof(Settings));
                file.Close();
                return settings;

            }
            catch (Exception)
            {
                log.log("Config file damaged");
                log.log($"Try to remove {PathToConfigFile}");
                Environment.Exit(1);

                return "newSetting ";
            }


        }
        public void SetSettings(object settings)
        {
            File.WriteAllText(PathToConfigFile, JsonConvert.SerializeObject(settings));
        }


    }
}
