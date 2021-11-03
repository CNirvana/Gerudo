using System.Collections.Generic;

namespace Gerudo
{
    public class Animation
    {
        public string name;

        public float duration;

        public List<Channel> channels = new List<Channel>();

        public class Channel
        {
            public string boneName;

            public TranslationTimeline translationTimeline;

            public RotationTimeline rotationTimeline;

            public ScaleTimeline scaleTimeline;
        }
    }
}