using System.Numerics;

namespace Gerudo
{
    public class TranslationTimeline : Timeline<Vector3>
    {
        public override Vector3 Sample(float currentTime)
        {
            if (frames.Length == 1 || currentTime <= frames[0].time)
            {
                return frames[0].value;
            }

            if (currentTime >= frames[frames.Length - 1].time)
            {
                return frames[frames.Length - 1].value;
            }

            int index = 0;
            for (int i = 0; i < frames.Length - 1; i++)
            {
                if (currentTime < frames[i + 1].time)
                {
                    index = i;
                    break;
                }
            }

            ref var currentFrame = ref frames[index];
            ref var nextFrame = ref frames[index + 1];

            float delta = (currentTime - currentFrame.time) / (nextFrame.time - currentFrame.time);

            return Vector3.Lerp(currentFrame.value, nextFrame.value, delta);
        }
    }
}