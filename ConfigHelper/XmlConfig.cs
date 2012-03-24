using System.IO;
using System.Linq;
using System.Xml;

namespace ConfigHelper
{
    /// <summary>
    /// Loads the XmlConfig file.
    /// </summary>
    public class XmlConfig : IBaseConfigType, IConfigType<XmlConfiguration>  {
        public virtual XmlConfiguration LoadConfig(string configName = "", string path = "")
        {
            return GetCustomXmlConfig(configName, path);
        }

        /// <summary>
        /// Opens the xml file
        /// </summary>
        /// <param name="configName">Name of the xml file (without the extension)</param>
        /// <param name="path">Path to the xml file</param>
        /// <returns>XmlConfiguration</returns>
        private XmlConfiguration GetCustomXmlConfig(string configName, string path)
        {
            XmlConfiguration config;
            if (!ConfigExist(path, configName, ".xml"))
            {
                config = new XmlConfiguration(new XmlDocument(), Path.Combine(path, configName + ".xml"));
                //let's add the XML declaration section
                var xmlnode = config.XmlDocument.CreateNode(XmlNodeType.XmlDeclaration, "", "");
                config.XmlDocument.AppendChild(xmlnode);
                var rootElm = config.XmlDocument.CreateElement("", "appSettings", "");
                config.XmlDocument.AppendChild(rootElm);
            }
            else
            {
                config = new XmlConfiguration(new XmlDocument(), Path.Combine(path, configName + ".xml"));
                using (XmlReader reader = new XmlTextReader(Path.Combine(path, configName + ".xml")))
                {
                    config.XmlDocument.Load(reader);
                }
                if (config.AppSettings.OriginalXmlElement == null)
                {
                    var rootElm = config.XmlDocument.DocumentElement;
                    var appSettingsSection = config.XmlDocument.CreateElement("", "appSettings", "");
                    rootElm.AppendChild(appSettingsSection);
                    config = new XmlConfiguration(config.XmlDocument, config.XmlDocumentFullPathName);
                }
            }

            return config;
        }

        /// <summary>
        /// Checks if the specified configuration exists.
        /// </summary>
        /// <param name="configPath">Path to the configuration file.</param>
        /// <param name="configName">Name of the configuration file.</param>
        /// <param name="extension"></param>
        /// <returns>Returns true if configuration exists.</returns>
        private bool ConfigExist(string configPath, string configName, string extension = ".config")
        {
            var dir = new DirectoryInfo(configPath);
            FileInfo[] files = dir.GetFiles("*" + extension);

            return files.Any(file => file.Name == configName + extension);
        }
    }
}