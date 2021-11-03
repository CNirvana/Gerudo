using System.Numerics;

namespace Gerudo
{
    public ref struct Frustum
    {
        public Plane top;

        public Plane bottom;

        public Plane left;

        public Plane right;

        public Plane near;

        public Plane far;
    }
}