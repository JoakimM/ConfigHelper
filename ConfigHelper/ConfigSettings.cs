using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Configuration;
using System.Xml;

namespace ConfigHelper {

    /// <summary>
    /// ConfigSetting helps loading and saving an objects properties marked with the attribute [IsSetting]
    /// to appSettings in a configuration file.
    /// <para>Only supports regular arrays</para>
    /// </summary>
    public static class ConfigSettings
    {
        /// <summary>
        /// Loads the key value pair represented by the object properties from the App.config AppSettings.
        /// </summary>
        /// <param name="obj">The class with the properties to be loaded to.</param>
        public static void LoadAppConfig(object obj) {
            TypeConvertAndLoad(obj, ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None));
        }

        /// <summary>
        /// Saves the properties of the object represented as key value pair to the app.config.
        /// </summary>
        /// <param name="obj">The class with the properties that is to be saved.</param>
        public static void SaveAppConfig(object obj) {
            TypeConvertAndSave(obj, ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None));
        }

        /// <summary>
        /// Loads the key value pair represented by the object properties from the Web.config AppSettings.
        /// </summary>
        /// <param name="obj">The class with the properties to be loaded to.</param>
        public static void LoadWebConfig(object obj) {
            TypeConvertAndLoad(obj, WebConfigurationManager.OpenWebConfiguration("~"));
        }

        /// <summary>
        /// Saves the properties of the object represented as key value pair to the Web.config.
        /// </summary>
        /// <param name="obj">The class with the properties that is to be saved.</param>
        public static void SaveWebConfig(object obj) {
            TypeConvertAndSave(obj, WebConfigurationManager.OpenWebConfiguration("~"));
        }

        /// <summary>
        /// Loads the keys represented by the objects properties from the config specified.
        /// </summary>
        /// <param name="configName">The name of the configuration to be loaded from.</param>
        /// <param name="obj">The class with the properties to be loaded to.</param>
        /// <param name="path">The path to the config file.</param>
        public static void LoadConfig(object obj, string configName, string path) {
            TypeConvertAndLoad(obj, GetCustomConfig(configName, path));
        }

        /// <summary>
        /// Saves the properties of the object represented as key value pair to the config specified.
        /// </summary>
        /// <param name="configName">The name of the configuration which the keys will be saved to.</param>
        /// <param name="obj">The class with the properties that is to be saved.</param>
        /// <param name="path">The path to the config file.</param>
        public static void SaveConfig(object obj, string configName, string path) {
            TypeConvertAndSave(obj, GetCustomConfig(configName,path));
        }

        /// <summary>
        /// Loads the keys represented by the objects properties from the xml file specified.
        /// </summary>
        /// <param name="configName">The name of the xml file to be loaded from.</param>
        /// <param name="obj">The class with the properties to be loaded to.</param>
        /// <param name="path">The path to the xml file.</param>
        public static void LoadXmlConfig(object obj, string configName, string path) {
            TypeConvertAndLoad(obj, GetCustomXmlConfig(configName, path));
        }

        /// <summary>
        /// Saves the properties of the object represented as key value pair to the xml file specified.
        /// If file doesn't exits it will create it, if file already exists but doesn't have an appSettings section it should create one.
        /// </summary>
        /// <param name="configName">The name of the xml file which the keys will be saved to.</param>
        /// <param name="obj">The class with the properties that is to be saved.</param>
        /// <param name="path">The path to the xml file.</param>
        public static void SaveXmlConfig(object obj, string configName, string path) {
            TypeConvertAndSave(obj, GetCustomXmlConfig(configName, path));
        }

        /// <summary>
        /// Checks if the specified configuration exists.
        /// </summary>
        /// <param name="configPath">Path to the configuration file.</param>
        /// <param name="configName">Name of the configuration file.</param>
        /// <param name="extension"></param>
        /// <returns>Returns true if configuration exists.</returns>
        public static bool ConfigExist(string configPath, string configName, string extension = ".config") {
            var dir = new DirectoryInfo(configPath);
            FileInfo[] files = dir.GetFiles("*" + extension);

            return files.Any(file => file.Name == configName + extension);
        }

        /// <summary>
        /// Opens the configuration file
        /// </summary>
        /// <param name="configName">Name of the configuration file (without the extension)</param>
        /// <param name="path">Path to the configuration file</param>
        /// <returns>Configuration</returns>
        private static Configuration GetCustomConfig(string configName, string path) {
            string fullPath = Path.Combine(path, string.Format("{0}.config", configName));
            var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = fullPath };
            return ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
        }

        /// <summary>
        /// Opens the xml file
        /// </summary>
        /// <param name="configName">Name of the xml file (without the extension)</param>
        /// <param name="path">Path to the xml file</param>
        /// <returns>XmlConfiguration</returns>
        private static XmlConfiguration GetCustomXmlConfig(string configName, string path) {
            XmlConfiguration config;
            if (!ConfigExist(path, configName, ".xml")) {
                config = new XmlConfiguration(new XmlDocument(), Path.Combine(path, configName + ".xml"));
                //let's add the XML declaration section
                var xmlnode = config.XmlDocument.CreateNode(XmlNodeType.XmlDeclaration, "", "");
                config.XmlDocument.AppendChild(xmlnode);
                var rootElm = config.XmlDocument.CreateElement("", "appSettings", "");
                config.XmlDocument.AppendChild(rootElm);
            }
            else {
                config = new XmlConfiguration(new XmlDocument(), Path.Combine(path, configName + ".xml"));
                using (XmlReader reader = new XmlTextReader(Path.Combine(path, configName + ".xml"))) {
                    config.XmlDocument.Load(reader);
                }
                if(config.AppSettings.OriginalXmlElement == null) {
                    var rootElm = config.XmlDocument.DocumentElement;
                    var appSettingsSection = config.XmlDocument.CreateElement("", "appSettings", "");
                    rootElm.AppendChild(appSettingsSection);
                    config = new XmlConfiguration(config.XmlDocument, config.XmlDocumentFullPathName);
                }
            }

            return config;
        }

        private static void TypeConvertAndLoad(object obj, Configuration config)
        {
            TypeConvertAndLoad(obj,
                               (property, targetType, typeConverter) =>
                               property.SetValue(obj,
                                                 typeConverter.ConvertFromString(
                                                     config.AppSettings.Settings[property.Name].Value), null),
                               (strList, index, property) => {
                                       var key =
                                           config.AppSettings.Settings.AllKeys.FirstOrDefault(
                                               k => k == property.Name + string.Format("[{0}]", index));
                                       if (!string.IsNullOrEmpty(key)) {
                                           strList.Add(config.AppSettings.Settings[key].Value);
                                       }
                                   });
        }

        private static void TypeConvertAndLoad(object obj, XmlConfiguration config)
        {
            TypeConvertAndLoad(obj,
                               (property, targetType, typeConverter) =>
                               property.SetValue(obj,
                                                 typeConverter.ConvertFromString(
                                                     config.AppSettings.Settings[property.Name].Value), null),
                               (strList, index, property) => {
                                       var key =
                                           config.AppSettings.Settings.AllKeys.FirstOrDefault(
                                               k => k == property.Name + string.Format("[{0}]", index));
                                       if (!string.IsNullOrEmpty(key)) {
                                           strList.Add(config.AppSettings.Settings[key].Value);
                                       }
                                   });
        }

        private static void TypeConvertAndLoad(object obj, Action<PropertyInfo, Type, TypeConverter> loadPropertyToObj,
            Action<List<string>, int, PropertyInfo> loadArrayPropertyToStrArray) {
            TypeConvert(obj,
                       (property, targetType, typeConverter) => {
                           if (!targetType.IsArray) {
                               loadPropertyToObj(property, targetType, typeConverter);
                           }
                           else {
                               var elmType = targetType.GetElementType();
                               TypeConverter tcItem = TypeDescriptor.GetConverter(elmType);
                               if (tcItem == null) return;
                               var resStrArr = new List<string>();
                               for (int i = 0; i < resStrArr.Count + 1; i++) {
                                   loadArrayPropertyToStrArray(resStrArr, i, property);
                               }

                               var resultArr = Array.CreateInstance(elmType, resStrArr.Count);
                               
                               for (int i = 0; i < resStrArr.Count; i++) {
                                   resultArr.SetValue(tcItem.ConvertFromString(resStrArr[i]), i);
                               }
                               property.SetValue(obj, resultArr, null);
                           }
                       });
        }

        private static void TypeConvertAndSave(object obj, Configuration config) {
            TypeConvertAndSave(obj, (property, valueType, typeConverter) => {
                                       config.AppSettings.Settings.Remove(property.Name);
                                       config.AppSettings.Settings.Add(property.Name,
                                                                       typeConverter.ConvertToString(null, null, property.GetValue(obj, null)));
                                   },
                               (array, property, typeConverter) => {
                                       config.AppSettings.Settings.RemoveSimilar(property.Name);
                                       for (int i = 0; i < array.Length; i++) {
                                           if (typeConverter.CanConvertTo(typeof (string))) {
                                               config.AppSettings.Settings.Add(
                                                   property.Name + string.Format("[{0}]", i),
                                                   typeConverter.ConvertToString(null, null, array.GetValue(i)));
                                           }
                                       }
                                   },
                               config.Save
                );
        }

        private static void TypeConvertAndSave(object obj, XmlConfiguration config) {
            TypeConvertAndSave(obj, (property, valueType, typeConverter) => {
                                       config.AppSettings.Settings.Remove(property.Name);
                                       config.AppSettings.Settings.Add(property.Name,
                                                                       typeConverter.ConvertToString(null, null, property.GetValue(obj, null)));
                                   },
                               (array, property, typeConverter) => {
                                       config.AppSettings.Settings.RemoveSimilar(property.Name);
                                       for (int i = 0; i < array.Length; i++) {
                                           if (typeConverter.CanConvertTo(typeof (string))) {
                                               config.AppSettings.Settings.Add(
                                                   property.Name + string.Format("[{0}]", i),
                                                   typeConverter.ConvertToString(null, null, array.GetValue(i)));
                                           }
                                       }
                                   },
                               config.Save
                );
        }

        private static void TypeConvertAndSave(object obj, Action<PropertyInfo, Type, TypeConverter> addPropertyToConfig, 
            Action<Array, PropertyInfo, TypeConverter> addArrayPropertyToConfig, Action saveConfig)
        {
            TypeConvert(obj,
                        (property, valueType, typeConverter) => {
                            if (!valueType.IsArray) {
                                addPropertyToConfig(property, valueType, typeConverter);
                            }
                            else {
                                var arr = property.GetValue(obj, null) as Array;
                                if (arr == null) return;
                                TypeConverter tcItem = TypeDescriptor.GetConverter(arr.GetType().GetElementType());
                                addArrayPropertyToConfig(arr, property, tcItem);
                            }
                        },
                        saveConfig);
        }

        private static void TypeConvert(object obj, Action<PropertyInfo, Type, TypeConverter> manipulateConfigValue, Action saveConfig = null) {
            Type t = obj.GetType();
            PropertyInfo[] properties = t.GetProperties();
            foreach (PropertyInfo property in properties) {
                if (saveConfig != null && !property.CanRead) continue;
                if (saveConfig == null && !property.CanWrite) continue;

                Type valueType = property.PropertyType;
                TypeConverter tc = TypeDescriptor.GetConverter(valueType);
                if (tc == null) throw new ArgumentNullException("The type converter is null and won't be able to convert.");
                object[] attributes = property.GetCustomAttributes(typeof(IsSetting), true);
                foreach (Attribute attribute in attributes) {
                    if ((saveConfig != null && (tc.CanConvertTo(typeof(string)) || valueType.IsArray)) || 
                        (saveConfig == null && (tc.CanConvertFrom(typeof(string)) || valueType.IsArray)))
                    {
                        manipulateConfigValue(property, valueType, tc);
                    }
                    else if (!tc.CanConvertTo(typeof(string))) {
                        throw new ConvertArgumentException(string.Format("Unable to convert the value from key: {0} to {1}.", property.Name, valueType.GetType()));
                    }
                }
                if(saveConfig != null)
                    saveConfig();
            }
        }
    }
}