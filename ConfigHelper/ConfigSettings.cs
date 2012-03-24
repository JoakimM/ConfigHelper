using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Management.Instrumentation;
using System.Reflection;

namespace ConfigHelper {
    //TODO: See if more name changes should be done

    /// <summary>
    /// ConfigSettings helps loading and saving an objects properties marked with the attribute [IsSetting]
    /// to appSettings in a configuration file.
    /// <para>Only supports regular arrays</para>
    /// </summary>
    public class ConfigSettings : IConfigSettings
    {
        private Dictionary<Type, IBaseConfigType> _configTypes = new Dictionary<Type, IBaseConfigType>();

        /// <summary>
        /// Default constructor, registers the following: AppConfig, WebConfig,CustomConfig and XmlConfig.
        /// </summary>
        public ConfigSettings()
        {
            RegisterConfigType(new AppConfig());
            RegisterConfigType(new WebConfig());
            RegisterConfigType(new CustomConfig());
            RegisterConfigType(new XmlConfig());
        }

        public virtual void RegisterConfigType(IBaseConfigType configType)
        {
            _configTypes.Add(configType.GetType(), configType);
        }

        public virtual void ClearConfigTypes()
        {
            _configTypes.Clear();
        }

        public virtual void Load<T>(object obj, string configName = "", string path = "") where T : class, IBaseConfigType
        {
            var typeOfT = typeof(T);
            var configType = _configTypes[typeOfT];
            if (configType == null) throw new InstanceNotFoundException(string.Format("Instance of type {0} was not found", typeOfT.Name));

            if (configType is IConfigType<Configuration>)
                TypeConvertAndLoad(obj, ((IConfigType<Configuration>)configType).LoadConfig(configName, path));
            if (configType is IConfigType<XmlConfiguration>)
                TypeConvertAndLoad(obj, ((IConfigType<XmlConfiguration>)configType).LoadConfig(configName, path));
        }

        public virtual void Save<T>(object obj, string configName = "", string path = "") where T : class, IBaseConfigType
        {
            var typeOfT = typeof(T);
            var configType = _configTypes[typeOfT];
            if (configType == null) throw new InstanceNotFoundException(string.Format("Instance of type {0} was not found", typeOfT.Name));

            if (configType is IConfigType<Configuration>)
                TypeConvertAndSave(obj, ((IConfigType<Configuration>)configType).LoadConfig(configName, path));
            if (configType is IConfigType<XmlConfiguration>)
                TypeConvertAndSave(obj, ((IConfigType<XmlConfiguration>)configType).LoadConfig(configName, path));
        }

        private void TypeConvertAndLoad(object obj, Configuration config)
        {
            TypeConvertAndLoad(obj,
                               (property, targetType, typeConverter) =>
                               property.SetValue(obj,
                                                 typeConverter.ConvertFromString(
                                                     config.AppSettings.Settings[property.Name].Value), null),
                               (strList, index, property) =>
                               {
                                   var key =
                                       config.AppSettings.Settings.AllKeys.FirstOrDefault(
                                           k => k == property.Name + string.Format("[{0}]", index));
                                   if (!string.IsNullOrEmpty(key))
                                   {
                                       strList.Add(config.AppSettings.Settings[key].Value);
                                   }
                               });
        }

        private void TypeConvertAndLoad(object obj, XmlConfiguration config)
        {
            TypeConvertAndLoad(obj,
                               (property, targetType, typeConverter) =>
                               property.SetValue(obj,
                                                 typeConverter.ConvertFromString(
                                                     config.AppSettings.Settings[property.Name].Value), null),
                               (strList, index, property) =>
                               {
                                   var key =
                                       config.AppSettings.Settings.AllKeys.FirstOrDefault(
                                           k => k == property.Name + string.Format("[{0}]", index));
                                   if (!string.IsNullOrEmpty(key))
                                   {
                                       strList.Add(config.AppSettings.Settings[key].Value);
                                   }
                               });
        }

        private void TypeConvertAndLoad(object obj, Action<PropertyInfo, Type, TypeConverter> loadPropertyToObj,
            Action<List<string>, int, PropertyInfo> loadArrayPropertyToStrArray)
        {
            TypeConvert(obj,
                       (property, targetType, typeConverter) =>
                       {
                           if (!targetType.IsArray)
                           {
                               loadPropertyToObj(property, targetType, typeConverter);
                           }
                           else
                           {
                               var elmType = targetType.GetElementType();
                               if (elmType == null) return;
                               TypeConverter tcItem = TypeDescriptor.GetConverter(elmType);
                               var resStrArr = new List<string>();
                               for (int i = 0; i < resStrArr.Count + 1; i++)
                               {
                                   loadArrayPropertyToStrArray(resStrArr, i, property);
                               }

                               var resultArr = Array.CreateInstance(elmType, resStrArr.Count);

                               for (int i = 0; i < resStrArr.Count; i++)
                               {
                                   resultArr.SetValue(tcItem.ConvertFromString(resStrArr[i]), i);
                               }
                               property.SetValue(obj, resultArr, null);
                           }
                       });
        }

        private void TypeConvertAndSave(object obj, Configuration config)
        {
            TypeConvertAndSave(obj, (property, valueType, typeConverter) =>
            {
                config.AppSettings.Settings.Remove(property.Name);
                config.AppSettings.Settings.Add(property.Name,
                                                typeConverter.ConvertToString(null, null, property.GetValue(obj, null)));
            },
                               (array, property, typeConverter) =>
                               {
                                   config.AppSettings.Settings.RemoveSimilar(property.Name);
                                   for (int i = 0; i < array.Length; i++)
                                   {
                                       if (typeConverter.CanConvertTo(typeof(string)))
                                       {
                                           config.AppSettings.Settings.Add(
                                               property.Name + string.Format("[{0}]", i),
                                               typeConverter.ConvertToString(null, null, array.GetValue(i)));
                                       }
                                   }
                               },
                               config.Save
                );
        }

        private void TypeConvertAndSave(object obj, XmlConfiguration config)
        {
            TypeConvertAndSave(obj, (property, valueType, typeConverter) =>
            {
                config.AppSettings.Settings.Remove(property.Name);
                config.AppSettings.Settings.Add(property.Name,
                                                typeConverter.ConvertToString(null, null, property.GetValue(obj, null)));
            },
                               (array, property, typeConverter) =>
                               {
                                   config.AppSettings.Settings.RemoveSimilar(property.Name);
                                   for (int i = 0; i < array.Length; i++)
                                   {
                                       if (typeConverter.CanConvertTo(typeof(string)))
                                       {
                                           config.AppSettings.Settings.Add(
                                               property.Name + string.Format("[{0}]", i),
                                               typeConverter.ConvertToString(null, null, array.GetValue(i)));
                                       }
                                   }
                               },
                               config.Save
                );
        }

        private void TypeConvertAndSave(object obj, Action<PropertyInfo, Type, TypeConverter> addPropertyToConfig,
            Action<Array, PropertyInfo, TypeConverter> addArrayPropertyToConfig, Action saveConfig)
        {
            TypeConvert(obj,
                        (property, valueType, typeConverter) =>
                        {
                            if (!valueType.IsArray)
                            {
                                addPropertyToConfig(property, valueType, typeConverter);
                            }
                            else
                            {
                                var arr = property.GetValue(obj, null) as Array;
                                if (arr == null) return;
                                TypeConverter tcItem = TypeDescriptor.GetConverter(arr.GetType().GetElementType());
                                addArrayPropertyToConfig(arr, property, tcItem);
                            }
                        },
                        saveConfig);
        }

        private void TypeConvert(object obj, Action<PropertyInfo, Type, TypeConverter> manipulateConfigValue, Action saveConfig = null)
        {
            Type t = obj.GetType();
            PropertyInfo[] properties = t.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (saveConfig != null && !property.CanRead) continue;
                if (saveConfig == null && !property.CanWrite) continue;

                Type valueType = property.PropertyType;
                TypeConverter tc = TypeDescriptor.GetConverter(valueType);
                object[] attributes = property.GetCustomAttributes(typeof(IsSetting), true);
                foreach (Attribute attribute in attributes)
                {
                    if ((saveConfig != null && (tc.CanConvertTo(typeof(string)) || valueType.IsArray)) ||
                        (saveConfig == null && (tc.CanConvertFrom(typeof(string)) || valueType.IsArray)))
                    {
                        manipulateConfigValue(property, valueType, tc);
                    }
                    else if (!tc.CanConvertTo(typeof(string)))
                    {
                        throw new ConvertArgumentException(string.Format("Unable to convert the value from key: {0} to {1}.", property.Name, valueType));
                    }
                }
                if (saveConfig != null)
                    saveConfig();
            }
        } 
    }
}