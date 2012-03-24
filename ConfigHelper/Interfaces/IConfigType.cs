namespace ConfigHelper
{
    /// <summary>
    /// Needs to be inherited in order to load the config. 
    /// </summary>
    /// <remarks>
    /// If &lt;T&gt; is another type than Configuration or XmlConfiguration
    /// it will not work.
    /// </remarks>
    /// <typeparam name="T">The returning configuration (only System.Configuration.Configuration or ConfigHelper.XmlConfiguration)</typeparam>
    public interface IConfigType<T>
    {
        /// <summary>
        /// Gets the configuration file
        /// </summary>
        /// <param name="configName">Configuration name</param>
        /// <param name="path">Path to configuration</param>
        /// <returns>The returning configuration (only System.Configuration.Configuration or ConfigHelper.XmlConfiguration)</returns>
        T LoadConfig(string configName = "", string path = "");
    }
}