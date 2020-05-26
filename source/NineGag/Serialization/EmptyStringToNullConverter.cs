
#region Using Directives

using System;
using Newtonsoft.Json;

#endregion

namespace NineGag.Serialization
{
    /// <summary>
    /// Represents a JSON converter, which parses empty strings as null. In the 9GAG API, there are often empty string, when <c>null</c>
    /// would be more appropriate.
    /// </summary>
    internal class EmptyStringToNullConverter : JsonConverter
    {
        #region JsonConverter Implementation

        /// <summary>
        /// Determines whether this converter can convert the specified type.
        /// </summary>
        /// <param name="objectType">The type for which is to be checked whether it can be converted by this converter.</param>
        /// <returns>Returns <c>true</c> if the object type is string and <c>false</c> otherwise.</returns>
        public override bool CanConvert(Type objectType) => objectType == typeof(string);

        /// <summary>
        /// Reads the JSON value. Converts non-empty strings to strings and empty strings to <c>null</c>.
        /// </summary>
        /// <param name="reader">The reader that is used to read the value.</param>
        /// <param name="objectType">The type of the property into which the value is to be converted.</param>
        /// <param name="existingValue">The already existing value of the property into which the value is to be deserialized.</param>
        /// <param name="serializer">The serializer that is used.</param>
        /// <returns>
        /// Returns the string if the value is a non-empty string and <c>null</c> if the string is <c>null</c>, empty, or white space.
        /// </returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // If the value is null anyway, then null is returned
            if (reader.Value == null)
                return null;

            // Reads the string value
            string text = reader.Value.ToString();

            // If the string is empty or only consists of white spaces, then null is also returned, otherwise the string is returned
            if (string.IsNullOrWhiteSpace(text))
                return null;
            return text;
        }

        /// <summary>
        /// Is not implemented.
        /// </summary>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();

        /// <summary>
        /// Gets a value that determines whether this converter can write values. Returns <c>false</c>, because this is a read-only
        /// converter.
        /// </summary>
        public override bool CanWrite { get => false; }

        #endregion
    }
}
