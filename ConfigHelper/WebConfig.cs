using System.Configuration;
using System.Web.Configuration;

namespace ConfigHelper
{
    /// <summary>
    /// Gets the WebConfig file.
    /// </summary>
    public class WebConfig : IBaseConfigType, IConfigType<Configuration>  {
        public virtual Configuration LoadConfig(string configName = "", string path = "")
        {
            return WebConfigurationManager.OpenWebConfiguration("~");
        }
    }
}