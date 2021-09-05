using System.Numerics;

namespace Gerudo
{
    public class Transform
    {
        public Vector3 Position { get; set; } = Vector3.Zero;

        public Quaternion Rotation { get; set; } = Quaternion.Identity;

        public Vector3 Scale { get; set; } = Vector3.One;

        public Vector3 UnitX => Vector3.Transform(Vector3.UnitX, Rotation);

        public Vector3 UnitY => Vector3.Transform(Vector3.UnitY, Rotation);

        public Vector3 UnitZ => Vector3.Transform(Vector3.UnitZ, Rotation);

        public Matrix4x4 GetLocalToWorldMatrix()
        {
            return Matrix4x4.CreateTranslation(Position)
                * Matrix4x4.CreateScale(Scale)
                * Matrix4x4.CreateFromQuaternion(Rotation);
        }
    }
}