// The SHA3 doesn't create .Net Standard package.
// This is a copy of https://bitbucket.org/jdluzen/sha3/raw/d1fd55dc225d18a7fb61515b62d3c8f164d2e788/SHA3Managed/SHA3Managed.cs

#pragma warning disable IDE0011 // Add braces
#pragma warning disable IDE0059 // Unnecessary assignment of a value
#pragma warning disable IDE1006 // Naming Styles

namespace Ipfs.Cryptography;

internal partial class KeccakManaged : Keccak
{
    public KeccakManaged(int hashBitLength)
        : base(hashBitLength)
    {
    }

    protected override void HashCore(byte[] array, int ibStart, int cbSize)
    {
        base.HashCore(array, ibStart, cbSize);
        if (cbSize == 0)
            return;
        int sizeInBytes = SizeInBytes;
        Buffer ??= new byte[sizeInBytes];
        int stride = sizeInBytes >> 3;
        ulong[] utemps = new ulong[stride];
        if (BuffLength == sizeInBytes)
            throw new Exception("Unexpected error, the internal buffer is full");
        AddToBuffer(array, ref ibStart, ref cbSize);
        if (BuffLength == sizeInBytes)//buffer full
        {
            System.Buffer.BlockCopy(Buffer, 0, utemps, 0, sizeInBytes);
            KeccakF(utemps, stride);
            BuffLength = 0;
        }
        for (; cbSize >= sizeInBytes; cbSize -= sizeInBytes, ibStart += sizeInBytes)
        {
            System.Buffer.BlockCopy(array, ibStart, utemps, 0, sizeInBytes);
            KeccakF(utemps, stride);
        }
        if (cbSize > 0)//some left over
        {
            System.Buffer.BlockCopy(array, ibStart, Buffer, BuffLength, cbSize);
            BuffLength += cbSize;
        }
    }

    protected override byte[] HashFinal()
    {
        int sizeInBytes = SizeInBytes;
        byte[] outb = new byte[HashByteLength];
        //    padding
        if (Buffer is null)
            Buffer = new byte[sizeInBytes];
        else
            Array.Clear(Buffer, BuffLength, sizeInBytes - BuffLength);
        Buffer[BuffLength++] = 1;
        Buffer[sizeInBytes - 1] |= 0x80;
        int stride = sizeInBytes >> 3;
        ulong[] utemps = new ulong[stride];
        System.Buffer.BlockCopy(Buffer, 0, utemps, 0, sizeInBytes);
        KeccakF(utemps, stride);
        System.Buffer.BlockCopy(State, 0, outb, 0, HashByteLength);
        return outb;
    }

    private void KeccakF(ulong[] inb, int laneCount)
    {
        while (--laneCount >= 0)
            State[laneCount] ^= inb[laneCount];
        ulong Aba, Abe, Abi, Abo, Abu;
        ulong Aga, Age, Agi, Ago, Agu;
        ulong Aka, Ake, Aki, Ako, Aku;
        ulong Ama, Ame, Ami, Amo, Amu;
        ulong Asa, Ase, Asi, Aso, Asu;
        ulong BCa, BCe, BCi, BCo, BCu;
        ulong Da, De, Di, Do, Du;
        ulong Eba, Ebe, Ebi, Ebo, Ebu;
        ulong Ega, Ege, Egi, Ego, Egu;
        ulong Eka, Eke, Eki, Eko, Eku;
        ulong Ema, Eme, Emi, Emo, Emu;
        ulong Esa, Ese, Esi, Eso, Esu;
        int round = laneCount;

        //copyFromState(A, state)
        Aba = State[0];
        Abe = State[1];
        Abi = State[2];
        Abo = State[3];
        Abu = State[4];
        Aga = State[5];
        Age = State[6];
        Agi = State[7];
        Ago = State[8];
        Agu = State[9];
        Aka = State[10];
        Ake = State[11];
        Aki = State[12];
        Ako = State[13];
        Aku = State[14];
        Ama = State[15];
        Ame = State[16];
        Ami = State[17];
        Amo = State[18];
        Amu = State[19];
        Asa = State[20];
        Ase = State[21];
        Asi = State[22];
        Aso = State[23];
        Asu = State[24];

        for (round = 0; round < KeccakNumberOfRounds; round += 2)
        {
            //    prepareTheta
            BCa = Aba ^ Aga ^ Aka ^ Ama ^ Asa;
            BCe = Abe ^ Age ^ Ake ^ Ame ^ Ase;
            BCi = Abi ^ Agi ^ Aki ^ Ami ^ Asi;
            BCo = Abo ^ Ago ^ Ako ^ Amo ^ Aso;
            BCu = Abu ^ Agu ^ Aku ^ Amu ^ Asu;

            //thetaRhoPiChiIotaPrepareTheta(round  , A, E)
            Da = BCu ^ ROL(BCe, 1);
            De = BCa ^ ROL(BCi, 1);
            Di = BCe ^ ROL(BCo, 1);
            Do = BCi ^ ROL(BCu, 1);
            Du = BCo ^ ROL(BCa, 1);

            Aba ^= Da;
            BCa = Aba;
            Age ^= De;
            BCe = ROL(Age, 44);
            Aki ^= Di;
            BCi = ROL(Aki, 43);
            Amo ^= Do;
            BCo = ROL(Amo, 21);
            Asu ^= Du;
            BCu = ROL(Asu, 14);
            Eba = BCa ^ ((~BCe) & BCi);
            Eba ^= RoundConstants[round];
            Ebe = BCe ^ ((~BCi) & BCo);
            Ebi = BCi ^ ((~BCo) & BCu);
            Ebo = BCo ^ ((~BCu) & BCa);
            Ebu = BCu ^ ((~BCa) & BCe);

            Abo ^= Do;
            BCa = ROL(Abo, 28);
            Agu ^= Du;
            BCe = ROL(Agu, 20);
            Aka ^= Da;
            BCi = ROL(Aka, 3);
            Ame ^= De;
            BCo = ROL(Ame, 45);
            Asi ^= Di;
            BCu = ROL(Asi, 61);
            Ega = BCa ^ ((~BCe) & BCi);
            Ege = BCe ^ ((~BCi) & BCo);
            Egi = BCi ^ ((~BCo) & BCu);
            Ego = BCo ^ ((~BCu) & BCa);
            Egu = BCu ^ ((~BCa) & BCe);

            Abe ^= De;
            BCa = ROL(Abe, 1);
            Agi ^= Di;
            BCe = ROL(Agi, 6);
            Ako ^= Do;
            BCi = ROL(Ako, 25);
            Amu ^= Du;
            BCo = ROL(Amu, 8);
            Asa ^= Da;
            BCu = ROL(Asa, 18);
            Eka = BCa ^ ((~BCe) & BCi);
            Eke = BCe ^ ((~BCi) & BCo);
            Eki = BCi ^ ((~BCo) & BCu);
            Eko = BCo ^ ((~BCu) & BCa);
            Eku = BCu ^ ((~BCa) & BCe);

            Abu ^= Du;
            BCa = ROL(Abu, 27);
            Aga ^= Da;
            BCe = ROL(Aga, 36);
            Ake ^= De;
            BCi = ROL(Ake, 10);
            Ami ^= Di;
            BCo = ROL(Ami, 15);
            Aso ^= Do;
            BCu = ROL(Aso, 56);
            Ema = BCa ^ ((~BCe) & BCi);
            Eme = BCe ^ ((~BCi) & BCo);
            Emi = BCi ^ ((~BCo) & BCu);
            Emo = BCo ^ ((~BCu) & BCa);
            Emu = BCu ^ ((~BCa) & BCe);

            Abi ^= Di;
            BCa = ROL(Abi, 62);
            Ago ^= Do;
            BCe = ROL(Ago, 55);
            Aku ^= Du;
            BCi = ROL(Aku, 39);
            Ama ^= Da;
            BCo = ROL(Ama, 41);
            Ase ^= De;
            BCu = ROL(Ase, 2);
            Esa = BCa ^ ((~BCe) & BCi);
            Ese = BCe ^ ((~BCi) & BCo);
            Esi = BCi ^ ((~BCo) & BCu);
            Eso = BCo ^ ((~BCu) & BCa);
            Esu = BCu ^ ((~BCa) & BCe);

            //    prepareTheta
            BCa = Eba ^ Ega ^ Eka ^ Ema ^ Esa;
            BCe = Ebe ^ Ege ^ Eke ^ Eme ^ Ese;
            BCi = Ebi ^ Egi ^ Eki ^ Emi ^ Esi;
            BCo = Ebo ^ Ego ^ Eko ^ Emo ^ Eso;
            BCu = Ebu ^ Egu ^ Eku ^ Emu ^ Esu;

            //thetaRhoPiChiIotaPrepareTheta(round+1, E, A)
            Da = BCu ^ ROL(BCe, 1);
            De = BCa ^ ROL(BCi, 1);
            Di = BCe ^ ROL(BCo, 1);
            Do = BCi ^ ROL(BCu, 1);
            Du = BCo ^ ROL(BCa, 1);

            Eba ^= Da;
            BCa = Eba;
            Ege ^= De;
            BCe = ROL(Ege, 44);
            Eki ^= Di;
            BCi = ROL(Eki, 43);
            Emo ^= Do;
            BCo = ROL(Emo, 21);
            Esu ^= Du;
            BCu = ROL(Esu, 14);
            Aba = BCa ^ ((~BCe) & BCi);
            Aba ^= RoundConstants[round + 1];
            Abe = BCe ^ ((~BCi) & BCo);
            Abi = BCi ^ ((~BCo) & BCu);
            Abo = BCo ^ ((~BCu) & BCa);
            Abu = BCu ^ ((~BCa) & BCe);

            Ebo ^= Do;
            BCa = ROL(Ebo, 28);
            Egu ^= Du;
            BCe = ROL(Egu, 20);
            Eka ^= Da;
            BCi = ROL(Eka, 3);
            Eme ^= De;
            BCo = ROL(Eme, 45);
            Esi ^= Di;
            BCu = ROL(Esi, 61);
            Aga = BCa ^ ((~BCe) & BCi);
            Age = BCe ^ ((~BCi) & BCo);
            Agi = BCi ^ ((~BCo) & BCu);
            Ago = BCo ^ ((~BCu) & BCa);
            Agu = BCu ^ ((~BCa) & BCe);

            Ebe ^= De;
            BCa = ROL(Ebe, 1);
            Egi ^= Di;
            BCe = ROL(Egi, 6);
            Eko ^= Do;
            BCi = ROL(Eko, 25);
            Emu ^= Du;
            BCo = ROL(Emu, 8);
            Esa ^= Da;
            BCu = ROL(Esa, 18);
            Aka = BCa ^ ((~BCe) & BCi);
            Ake = BCe ^ ((~BCi) & BCo);
            Aki = BCi ^ ((~BCo) & BCu);
            Ako = BCo ^ ((~BCu) & BCa);
            Aku = BCu ^ ((~BCa) & BCe);

            Ebu ^= Du;
            BCa = ROL(Ebu, 27);
            Ega ^= Da;
            BCe = ROL(Ega, 36);
            Eke ^= De;
            BCi = ROL(Eke, 10);
            Emi ^= Di;
            BCo = ROL(Emi, 15);
            Eso ^= Do;
            BCu = ROL(Eso, 56);
            Ama = BCa ^ ((~BCe) & BCi);
            Ame = BCe ^ ((~BCi) & BCo);
            Ami = BCi ^ ((~BCo) & BCu);
            Amo = BCo ^ ((~BCu) & BCa);
            Amu = BCu ^ ((~BCa) & BCe);

            Ebi ^= Di;
            BCa = ROL(Ebi, 62);
            Ego ^= Do;
            BCe = ROL(Ego, 55);
            Eku ^= Du;
            BCi = ROL(Eku, 39);
            Ema ^= Da;
            BCo = ROL(Ema, 41);
            Ese ^= De;
            BCu = ROL(Ese, 2);
            Asa = BCa ^ ((~BCe) & BCi);
            Ase = BCe ^ ((~BCi) & BCo);
            Asi = BCi ^ ((~BCo) & BCu);
            Aso = BCo ^ ((~BCu) & BCa);
            Asu = BCu ^ ((~BCa) & BCe);
        }

        //copyToState(state, A)
        State[0] = Aba;
        State[1] = Abe;
        State[2] = Abi;
        State[3] = Abo;
        State[4] = Abu;
        State[5] = Aga;
        State[6] = Age;
        State[7] = Agi;
        State[8] = Ago;
        State[9] = Agu;
        State[10] = Aka;
        State[11] = Ake;
        State[12] = Aki;
        State[13] = Ako;
        State[14] = Aku;
        State[15] = Ama;
        State[16] = Ame;
        State[17] = Ami;
        State[18] = Amo;
        State[19] = Amu;
        State[20] = Asa;
        State[21] = Ase;
        State[22] = Asi;
        State[23] = Aso;
        State[24] = Asu;

    }
}
