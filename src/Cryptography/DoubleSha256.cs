using System.Security.Cryptography;

namespace Ipfs.Cryptography;

internal sealed class DoubleSha256 : HashAlgorithm, IDisposable
{
    private readonly HashAlgorithm _digest = SHA256.Create();

    private byte[]? _round1;

    public override int HashSize => _digest.HashSize;

    public override void Initialize() => _digest.Initialize();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _digest?.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override void HashCore(byte[] array, int ibStart, int cbSize)
    {
        if (_round1 is not null)
        {
            throw new NotSupportedException("Already called.");
        }

        _round1 = _digest.ComputeHash(array, ibStart, cbSize);
    }

    protected override byte[] HashFinal()
    {
        _digest.Initialize();
        return _digest.ComputeHash(_round1!);
    }
}
