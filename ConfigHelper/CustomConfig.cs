using System.Configuration;
using System.IO;

namespace ConfigHelper
{
    /// <summary>
    /// Loads the CustomConfig file.
    /// </summary>
    public class CustomConfig : IBaseConfigType, IConfigType<Configuration>  {
        public virtual Configuration LoadConfig(string configName = "", string path = "")
        {
            return GetCustomConfig(configName, path);
        }

        /// <summary>
        /// Opens the configuration file
        /// </summary>
        /// <param name="configName">Name of the configuration file (without the extension)</param>
        /// <param name="path">Path to the configuration file</param>
        /// <returns>Configuration</returns>
        private Configuration GetCustomConfig(string configName, string path)
        {
            string fullPath = Path.Combine(path, string.Format("{0}.config", configName));
            var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = fullPath };
            return ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
        }
    }
}