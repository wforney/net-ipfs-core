﻿using Google.Protobuf;
using Ipfs.Registry;

namespace Ipfs;

/// <summary>
///   Wraps other formats with a tiny bit of self-description.
/// </summary>
/// <remarks>
///   <b>MultiCodec</b> is a self-describing multiformat, it wraps other formats with a
///   tiny bit of self-description. A multicodec identifier is both a varint and the code
///   identifying the following data.
///   <para>
///   Adds the following extension methods to <see cref="Stream"/>
///    <list type="bullet">
///      <item><description>ReadMultiCodec</description></item>
///      <item><description><see cref="WriteMultiCodec"/></description></item>
///    </list>
///   </para>
/// </remarks>
/// <seealso href="https://github.com/multiformats/multicodec"/>
/// <seealso cref="Registry.Codec"/>
public static class MultiCodec
{
    /// <summary>
    ///   Reads a <see cref="Codec"/> from the <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">
    ///   A multicodec encoded <see cref="Stream"/>.
    /// </param>
    /// <returns>The codec.</returns>
    /// <remarks>
    ///   If the <b>code</b> does not exist, a new <see cref="Codec"/> is
    ///   registered with the <see cref="Codec.Name"/> "codec-x"; where
    ///   'x' is the code's decimal represention.
    /// </remarks>
    public static Codec ReadMultiCodec(this Stream stream)
    {
        int code = stream.ReadVarint32();
        _ = Codec.Codes.TryGetValue(code, out Codec? codec);
        codec ??= Codec.Register($"codec-{code}", code);
        return codec;
    }

    /// <summary>
    ///   Reads a <see cref="Codec"/> from the <see cref="CodedInputStream"/>.
    /// </summary>
    /// <param name="stream">
    ///   A multicodec encoded <see cref="CodedInputStream"/>.
    /// </param>
    /// <returns>The codec.</returns>
    /// <remarks>
    ///   If the <b>code</b> does not exist, a new <see cref="Codec"/> is
    ///   registered with the <see cref="Codec.Name"/> "codec-x"; where
    ///   'x' is the code's decimal represention.
    /// </remarks>
    public static Codec ReadMultiCodec(this CodedInputStream stream)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        int code = stream.ReadInt32();
        _ = Codec.Codes.TryGetValue(code, out Codec? codec);

        return codec ?? Codec.Register($"codec-{code}", code);
    }

    /// <summary>
    ///   Writes a <see cref="Codec"/> to the <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">
    ///   A multicodec encoded <see cref="Stream"/>.
    /// </param>
    /// <param name="name">
    ///   The <see cref="Codec.Name"/>.
    /// </param>
    /// <remarks>
    ///   Writes the <see cref="Varint"/> of the <see cref="Codec.Code"/> to
    ///   the <paramref name="stream"/>.
    /// </remarks>
    /// <exception cref="KeyNotFoundException">
    ///   When <paramref name="name"/> is not registered.
    /// </exception>
    public static void WriteMultiCodec(this Stream stream, string name)
    {
        if (!Codec.Names.TryGetValue(name, out Codec? codec))
        {
            throw new KeyNotFoundException($"Codec '{name}' is not registered.");
        }

        stream.WriteVarint(codec.Code);
    }
}
