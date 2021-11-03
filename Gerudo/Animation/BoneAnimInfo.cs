using System.Numerics;
using System.Runtime.CompilerServices;

namespace Gerudo
{
    public unsafe struct BoneAnimInfo
    {
        public Matrix4x4[] transformations;

        internal static BoneAnimInfo New()
        {
            return new BoneAnimInfo() { transformations = new Matrix4x4[64] };
        }

        public BoneTransformationData GetTransformationData()
        {
            BoneTransformationData b;
            fixed (Matrix4x4* ptr = transformations)
            {
                Unsafe.CopyBlock(&b, ptr, 64 * 64);
            }

            return b;
        }
    }
}