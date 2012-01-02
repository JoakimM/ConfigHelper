using System;

namespace ConfigHelper
{
    /// <summary>
    /// The exception thrown when propgram failed to convert a property to string and vice-versa.
    /// </summary>
    [Serializable]
    public class ConvertArgumentException : Exception {
        /// <summary>
        /// Base contstructor for ConvertArgumentException.
        /// </summary>
        /// <param name="msg">The message to be displayed when exception is thrown.</param>
        public ConvertArgumentException(string msg)
            : base(msg) { }
    }
}
