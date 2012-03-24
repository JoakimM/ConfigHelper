namespace ConfigHelper
{
    /// <summary>
    /// ConfigSettings helps loading and saving an objects properties marked with the attribute [IsSetting]
    /// to appSettings in a configuration file.
    /// </summary>
    public interface IConfigSettings
    {
        /// <summary>
        /// Loads the keys represented by the objects properties from the config specified.
        /// </summary>
        /// <param name="configName">The name of the configuration to be loaded from.</param>
        /// <param name="obj">The class with the properties to be loaded to.</param>
        /// <param name="path">The path to the config file.</param>
        void Load<T>(object obj, string configName = "", string path = "") where T : class, IBaseConfigType;

        /// <summary>
        /// Saves the properties of the object represented as key value pair to the config specified.
        /// </summary>
        /// <param name="configName">The name of the configuration which the keys will be saved to.</param>
        /// <param name="obj">The class with the properties that is to be saved.</param>
        /// <param name="path">The path to the config file.</param>
        void Save<T>(object obj, string configName = "", string path = "") where T : class, IBaseConfigType;

        /// <summary>
        /// Registers a configuration type which is used to get the configuration file (for now)
        /// </summary>
        /// <param name="configType">The type to be registered</param>
        void RegisterConfigType(IBaseConfigType configType);

        /// <summary>
        /// Clears all the registerd configuration types
        /// </summary>
        void ClearConfigTypes();
    }
}