using Newtonsoft.Json;

namespace Ipfs
{
    public partial class Cid
    {
        /// <summary>
        ///   Conversion of a <see cref="Cid"/> to and from JSON.
        /// </summary>
        /// <remarks>
        ///   The JSON is just a single string value.
        /// </remarks>
        public class CidJsonConverter : JsonConverter
        {
            /// <inheritdoc />
            public override bool CanConvert(Type objectType)
            {
                return true;
            }

            /// <inheritdoc />
            public override bool CanRead => true;

            /// <inheritdoc />
            public override bool CanWrite => true;

            /// <inheritdoc />
            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                var cid = value as Cid;
                writer.WriteValue(cid?.Encode());
            }

            /// <inheritdoc />
            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                return reader.Value is string s ? Decode(s) : null;
            }
        }

    }
}
