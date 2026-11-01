using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using NewtonsoftJson = Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ipfs.Serialization;

/// <summary>
/// Provides compatible JSON serialization for core IPFS value types.
/// </summary>
public static class JsonCompat
{
    private static readonly JsonSerializerOptions DefaultStjOptions = ConfigureDefaultConverters(new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    });

    private static readonly ConcurrentDictionary<System.Type, bool> RequiresNewtonsoft = new();

    /// <summary>
    /// Serializes a value using System.Text.Json when supported, otherwise Newtonsoft.Json.
    /// </summary>
    public static string Serialize(object? value, JsonSerializerOptions? stjOptions = null, NewtonsoftJson.JsonSerializerSettings? newtonsoftSettings = null, bool forceNewtonsoft = false)
    {
        if (value is null)
        {
            return "null";
        }

        System.Type type = value.GetType();
        if (!forceNewtonsoft && CanUseSystemTextJson(type))
        {
            return System.Text.Json.JsonSerializer.Serialize(value, stjOptions ?? DefaultStjOptions);
        }
        return NewtonsoftJson.JsonConvert.SerializeObject(value, newtonsoftSettings);
    }

    /// <summary>
    /// Deserializes a value using System.Text.Json when supported, otherwise Newtonsoft.Json.
    /// </summary>
    public static T? Deserialize<T>(string json, JsonSerializerOptions? stjOptions = null, NewtonsoftJson.JsonSerializerSettings? newtonsoftSettings = null, bool forceNewtonsoft = false)
    {
        System.Type t = typeof(T);
        if (!forceNewtonsoft && CanUseSystemTextJson(t))
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(json, stjOptions ?? DefaultStjOptions);
        }
        return NewtonsoftJson.JsonConvert.DeserializeObject<T>(json, newtonsoftSettings);
    }

    /// <summary>
    /// Deserializes a value of the specified type using the compatible serializer.
    /// </summary>
    public static object? Deserialize(string json, System.Type type, JsonSerializerOptions? stjOptions = null, NewtonsoftJson.JsonSerializerSettings? newtonsoftSettings = null, bool forceNewtonsoft = false)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(type);
#else
        if (type is null)
        {
            throw new System.ArgumentNullException(nameof(type));
        }
#endif
        if (!forceNewtonsoft && CanUseSystemTextJson(type))
        {
            return System.Text.Json.JsonSerializer.Deserialize(json, type, stjOptions ?? DefaultStjOptions);
        }
        return NewtonsoftJson.JsonConvert.DeserializeObject(json, type, newtonsoftSettings);
    }

    private static bool CanUseSystemTextJson(System.Type type)
    {
        return RequiresNewtonsoft.GetOrAdd(type, static t =>
        {
            if (typeof(JToken).IsAssignableFrom(t))
            {
                return false;
            }
            if (t.Assembly.FullName is not null && t.Assembly.FullName.StartsWith("Newtonsoft.Json", System.StringComparison.Ordinal))
            {
                return false;
            }
            foreach (System.Reflection.PropertyInfo p in t.GetProperties())
            {
                if (typeof(JToken).IsAssignableFrom(p.PropertyType))
                {
                    return false;
                }
            }
            return true;
        });
    }

    /// <summary>
    /// Adds the core value-type converters to the supplied System.Text.Json options.
    /// </summary>
    public static JsonSerializerOptions ConfigureDefaultConverters(JsonSerializerOptions options)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(options);
#else
        if (options is null)
        {
            throw new System.ArgumentNullException(nameof(options));
        }
#endif

        if (!options.Converters.Any(static converter => converter is CidJsonConverter))
        {
            options.Converters.Add(new CidJsonConverter());
        }
        if (!options.Converters.Any(static converter => converter is MultiHashJsonConverter))
        {
            options.Converters.Add(new MultiHashJsonConverter());
        }
        if (!options.Converters.Any(static converter => converter is MultiAddressJsonConverter))
        {
            options.Converters.Add(new MultiAddressJsonConverter());
        }
        return options;
    }

    private sealed class CidJsonConverter : JsonConverter<Cid>
    {
        public override Cid? Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException("Cid must be a JSON string.");
            }
            string? s = reader.GetString();
            return s is null ? null : Cid.Decode(s);
        }
        public override void Write(Utf8JsonWriter writer, Cid value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Encode());
        }
    }

    private sealed class MultiHashJsonConverter : JsonConverter<MultiHash>
    {
        public override MultiHash? Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException("MultiHash must be a JSON string.");
            }
            string? s = reader.GetString();
            return s is null ? null : new MultiHash(s);
        }
        public override void Write(Utf8JsonWriter writer, MultiHash value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    private sealed class MultiAddressJsonConverter : JsonConverter<MultiAddress>
    {
        public override MultiAddress? Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException("MultiAddress must be a JSON string.");
            }
            string? s = reader.GetString();
            return s is null ? null : new MultiAddress(s);
        }
        public override void Write(Utf8JsonWriter writer, MultiAddress value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
