using Newtonsoft.Json;

namespace Ipfs
{
    public partial class MultiHash
    {
        /// <summary>
        ///   Conversion of a <see cref="MultiHash"/> to and from JSON.
        /// </summary>
        /// <remarks>
        ///   The JSON is just a single string value.
        /// </remarks>
        private class Json : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return true;
            }
            public override bool CanRead => true;
            public override bool CanWrite => true;
            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                var mh = value as MultiHash;
                writer.WriteValue(mh?.ToString());
            }

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                return reader.Value is string s ? new MultiHash(s) : null;
            }
        }
    }
}
