using System;

namespace ConfigHelper
{
    /// <summary>
    /// Used to confirm that the property is a setting that should be saved/loaded
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IsSetting : Attribute { }
}