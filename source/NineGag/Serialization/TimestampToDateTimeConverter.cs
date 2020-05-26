
#region Using Directives

using System;
using Newtonsoft.Json;

#endregion

namespace NineGag.Serialization
{
    /// <summary>
    /// Represents a JSON converter, which parses UNIX timestamps in seconds to <see cref="DateTime"/> objects.
    /// </summary>
    internal class TimestampToDateTimeConverter : JsonConverter
    {
        #region Private Static Fields

        /// <summary>
        /// Contains the UNIX epoch as a <see cref="DateTime"/> object.
        /// </summary>
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        #endregion

        #region JsonConverter Implementation

        /// <summary>
        /// Determines whether this converter can convert the specified type.
        /// </summary>
        /// <param name="objectType">The type for which is to be checked whether it can be converted by this converter.</param>
        /// <returns>Returns <c>true</c> if the object type is <see cref="DateTime"/> and <c>false</c> otherwise.</returns>
        public override bool CanConvert(Type objectType) => objectType == typeof(DateTime);

        /// <summary>
        /// Reads the JSON value. Converts the UNIX timestamp in seconds to a <see cref="DateTime"/> objects.
        /// </summary>
        /// <param name="reader">The reader that is used to read the value.</param>
        /// <param name="objectType">The type of the property into which the value is to be converted.</param>
        /// <param name="existingValue">The already existing value of the property into which the value is to be deserialized.</param>
        /// <param name="serializer">The serializer that is used.</param>
        /// <returns>
        /// Returns the local date and time represented by the UNIX timestamp. If the timestamp is <c>null</c>, then the default value for
        /// <see cref="DateTime"/> is returned.
        /// </returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // If the timestamp is null, then an "empty" date time is returned
            if (reader.Value == null)
                return default(DateTime);

            // Reads the timestamp
            long timestamp = Convert.ToInt64(reader.Value);

            // Converts the UNIX timestamp to a local date time and returned it
            return TimestampToDateTimeConverter.epoch
                .AddSeconds(timestamp)
                .ToLocalTime();
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
