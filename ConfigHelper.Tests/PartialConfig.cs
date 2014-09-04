using System.Xml;

namespace ConfigHelper.Tests
{
  internal class PartialConfig : IBaseConfigType, IConfigType<XmlConfiguration>
  {
    public XmlConfiguration LoadConfig(string configName = "", string path = "")
    {
      return getCustomXmlConfig(configName, path);

    }

    private XmlConfiguration getCustomXmlConfig(string configName = "", string path = "")
    {
      var config = new XmlConfiguration(new XmlDocument(), "");
      var doc = config.XmlDocument;

      doc.AppendChild(doc.CreateNode(XmlNodeType.XmlDeclaration, "", ""));
      
      var appSettings = doc.CreateElement("", "appSettings", "");
      doc.AppendChild(appSettings);

      var setting = doc.CreateElement("add");
      setting.SetAttribute("key", "Item1");
      setting.SetAttribute("value", "1");
      appSettings.AppendChild(setting);

      return config;
    }
  }
}