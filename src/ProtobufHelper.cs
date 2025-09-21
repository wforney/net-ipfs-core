using System.Reflection;
using Google.Protobuf;

namespace Ipfs;

internal static class ProtobufHelper
{
    private static readonly MethodInfo WriteRawBytes = typeof(CodedOutputStream)
        .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
        .Single(m =>
            m.Name == "WriteRawBytes" && m.GetParameters().Length == 1
        );
    private static readonly MethodInfo ReadRawBytes = typeof(CodedInputStream)
        .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
        .Single(m =>
            m.Name == "ReadRawBytes"
        );

    public static void WriteSomeBytes(this CodedOutputStream stream, byte[] bytes) => _ = WriteRawBytes.Invoke(stream, [bytes]);

    public static byte[]? ReadSomeBytes(this CodedInputStream stream, int length) => (byte[]?)ReadRawBytes.Invoke(stream, [length]);
}
