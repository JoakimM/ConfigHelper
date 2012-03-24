using System.Configuration;

namespace ConfigHelper
{
    /// <summary>
    /// Loads the AppConfig file.
    /// </summary>
    public class AppConfig : IBaseConfigType, IConfigType<Configuration> {
        public virtual Configuration LoadConfig(string configName = "", string path = "")
        {
            return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }
    }
}