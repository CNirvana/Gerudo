namespace Gerudo
{
    public abstract class Timeline
    {
        // public string boneName { get; }
    }

    public abstract class Timeline<T> : Timeline
    {
        public Keyframe[] frames;

        public abstract T Sample(float currentTime);

        public struct Keyframe
        {
            public float time;

            public T value;
        }
    }
}