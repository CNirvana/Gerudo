using System.Collections.Generic;
using System.Numerics;

namespace Gerudo
{
    public class Skeleton
    {
        public Bone[] Bones { get; set; }

        internal Dictionary<string, int> BoneNameToIdx { get; set; }

        internal Matrix4x4 InverseRootTransform { get; set; }

        internal int FindBoneIndex(string name)
        {
            return BoneNameToIdx[name];
        }
    }
}