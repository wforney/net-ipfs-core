/*
 * The package was named SHA3 it is really Keccak
 * https://medium.com/@ConsenSys/are-you-really-using-sha-3-or-old-code-c5df31ad2b0
 * See
 *
 * The SHA3 package doesn't create .Net Standard package.
 * This is a copy of https://bitbucket.org/jdluzen/sha3/raw/d1fd55dc225d18a7fb61515b62d3c8f164d2e788/SHA3/SHA3.cs
 * with nullable modifications.
 */

namespace Ipfs.Cryptography;

internal abstract class Keccak : System.Security.Cryptography.HashAlgorithm
{
    public const int KeccakB = 1600;
    public const int KeccakLaneSizeInBits = 8 * 8;
    public const int KeccakNumberOfRounds = 24;
    public readonly ulong[] RoundConstants;

    protected byte[]? Buffer;
    protected int BuffLength;
    protected new ulong[] State = new ulong[5 * 5];  //1600 bits

    protected Keccak(int hashBitLength)
    {
        if (hashBitLength is not 224 and not 256 and not 384 and not 512)
        {
            throw new ArgumentException("hashBitLength must be 224, 256, 384, or 512", nameof(hashBitLength));
        }

        Initialize();
        HashSizeValue = hashBitLength;
        switch (hashBitLength)
        {
            case 224:
                KeccakR = 1152;
                break;

            case 256:
                KeccakR = 1088;
                break;

            case 384:
                KeccakR = 832;
                break;

            case 512:
                KeccakR = 576;
                break;

            default:
                break;
        }
        RoundConstants =
        [
            0x0000000000000001UL,
            0x0000000000008082UL,
            0x800000000000808aUL,
            0x8000000080008000UL,
            0x000000000000808bUL,
            0x0000000080000001UL,
            0x8000000080008081UL,
            0x8000000000008009UL,
            0x000000000000008aUL,
            0x0000000000000088UL,
            0x0000000080008009UL,
            0x000000008000000aUL,
            0x000000008000808bUL,
            0x800000000000008bUL,
            0x8000000000008089UL,
            0x8000000000008003UL,
            0x8000000000008002UL,
            0x8000000000000080UL,
            0x000000000000800aUL,
            0x800000008000000aUL,
            0x8000000080008081UL,
            0x8000000000008080UL,
            0x0000000080000001UL,
            0x8000000080008008UL
        ];
    }

    public override bool CanReuseTransform => true;
    public override byte[] Hash => HashValue!;
    public int HashByteLength => HashSizeValue / 8;
    public override int HashSize => HashSizeValue;
    public int KeccakR { get; protected set; }

    public int SizeInBytes => KeccakR / 8;

    public override void Initialize()
    {
        BuffLength = 0;
        HashValue = null;
    }

    protected void AddToBuffer(byte[] array, ref int offset, ref int count)
    {
        int amount = Math.Min(count, Buffer!.Length - BuffLength);
        System.Buffer.BlockCopy(array, offset, Buffer, BuffLength, amount);
        offset += amount;
        BuffLength += amount;
        count -= amount;
    }

    protected override void HashCore(byte[] array, int ibStart, int cbSize)
    {
        if (ibStart < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ibStart));
        }

        if (cbSize > array.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(cbSize));
        }

        if (ibStart + cbSize > array.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(ibStart), "ibStart or cbSize");
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
    protected ulong ROL(ulong a, int offset) =>
        (((a) << (offset % KeccakLaneSizeInBits)) ^ ((a) >> (KeccakLaneSizeInBits - (offset % KeccakLaneSizeInBits))))!;
}
