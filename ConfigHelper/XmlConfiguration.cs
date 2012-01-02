using System.Collections.Generic;
using System.Configuration;
using System.Xml;

namespace ConfigHelper
{
    /* 
     * These classes has been made instead of the extension methods, their sole purpose is
     * to get the same syntax for both XmlDocument and Configuration when getting values
     * adding values and removing values 
    */

    internal class XmlConfiguration
    {
        public readonly XmlDocument XmlDocument;
        public readonly string XmlDocumentFullPathName;

        public XmlConfiguration(XmlDocument doc, string xmlDocumentFullPathName) {
            XmlDocument = doc;
            XmlDocumentFullPathName = xmlDocumentFullPathName;
        }

        public XmlAppSettingsSection AppSettings {
            get {
                var appSettings = XmlDocument["appSettings"] ?? XmlDocument.DocumentElement["appSettings"];
                return new XmlAppSettingsSection(this, appSettings);
            }
        }

        public void Save() {
            XmlDocument.Save(XmlDocumentFullPathName);
        }
    }

    internal class XmlAppSettingsSection
    {
        public readonly XmlConfiguration OwnerDocument;
        public readonly XmlElement OriginalXmlElement;

        public XmlAppSettingsSection(XmlConfiguration ownerDocument, XmlElement xmlElement) {
            OriginalXmlElement = xmlElement;
            OwnerDocument = ownerDocument;
        }

        public KeyValueXmlConfigurationCollection Settings {
            get {
                var dictionary = new KeyValueConfigurationCollection();
                foreach (XmlElement add in OriginalXmlElement.ChildNodes) {
                    if (add.Name == "add") {
                        dictionary.Add(add.Attributes["key"].Value, add.Attributes["value"].Value);
                    }
                }

                return new KeyValueXmlConfigurationCollection(this, dictionary);
            }
        }
    }

    internal class KeyValueXmlConfigurationCollection {
        private readonly XmlAppSettingsSection _xmlAppSettings;
        private readonly KeyValueConfigurationCollection _originalCollection;
        public KeyValueXmlConfigurationCollection(XmlAppSettingsSection xmlAppSettings, KeyValueConfigurationCollection collection) {
            _xmlAppSettings = xmlAppSettings;
            _originalCollection = collection;
        }

        public KeyValueConfigurationElement this[string key] {
            get { return _originalCollection[key]; }
        }

        public string[] AllKeys { get { return _originalCollection.AllKeys; } }

        public void Add(string key, string value) {
            var notFound = true;
            foreach (XmlElement add in _xmlAppSettings.OriginalXmlElement.ChildNodes) {
                if (add.Attributes["key"].Value == key) {
                    notFound = false;
                }
            }

            var newAdd = _xmlAppSettings.OwnerDocument.XmlDocument.CreateElement("add");
            newAdd.SetAttribute("key", key);
            newAdd.SetAttribute("value", value);

            if (notFound) {
                _xmlAppSettings.OriginalXmlElement.AppendChild(newAdd);
                _originalCollection.Add(key, value);
            }
        }

        public void Remove(string key) {
            foreach (XmlElement add in _xmlAppSettings.OriginalXmlElement.ChildNodes) {
                if (add.Name == "add" && add.Attributes["key"].Value == key) {
                    _xmlAppSettings.OriginalXmlElement.RemoveChild(add);
                    _originalCollection.Remove(key);
                }
            }
        }

        public void RemoveSimilar(string key) {
            for (var i = 0; i < _xmlAppSettings.OriginalXmlElement.ChildNodes.Count; i++) {
                var add = _xmlAppSettings.OriginalXmlElement.ChildNodes[i];
                if (add.Name == "add" && add.Attributes["key"].Value.Contains(key))
                {
                    _xmlAppSettings.OriginalXmlElement.RemoveChild(add);
                    _originalCollection.Remove(key);
                }
            }
        }
    }

    internal static class ConfigurationExtensions {
        public static void RemoveSimilar(this KeyValueConfigurationCollection appSettingsCollection, string key) {
            var nameList = new List<string>();
            nameList.AddRange(appSettingsCollection.AllKeys);
            foreach(var item in nameList) {
                if (item.Contains(key)) {
                    appSettingsCollection.Remove(item);
                }
            }
        }
    }
}