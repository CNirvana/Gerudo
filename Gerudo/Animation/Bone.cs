using System.Numerics;

namespace Gerudo
{
    public struct Bone
    {
        public string name;

        public int parentIndex;

        public Matrix4x4 transform;

        public Matrix4x4 offset;
    }
}