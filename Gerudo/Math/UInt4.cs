using System;

namespace Gerudo
{
    public struct UInt4 : IEquatable<UInt4>
    {
        public uint X;

        public uint Y;

        public uint Z;

        public uint W;

        public UInt4(uint value)
        {
            X = Y = Z = W = value;
        }

        public UInt4(uint x, uint y, uint z, uint w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public bool Equals(UInt4 other)
        {
            return (X == other.X)
                && (Y == other.Y)
                && (Z == other.Z)
                && (W == other.W);
        }
    }
}