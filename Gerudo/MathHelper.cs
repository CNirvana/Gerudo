using System;

namespace Gerudo
{
    public static class MathHelper
    {
        public const float DEG2RAD = MathF.PI / 180f;

        public const float RAD2DEG = 180f / MathF.PI;

        public static float Clamp(float value, float min, float max)
        {
            return MathF.Max(MathF.Min(value, max), min);
        }
    }
}