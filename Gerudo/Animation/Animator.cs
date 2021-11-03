using System.Numerics;
using System.Runtime.CompilerServices;

namespace Gerudo
{
    public class Animator
    {
        public Animation CurrentAnimation { get; private set; }

        private Skeleton _skeleton;

        private BoneAnimInfo _animInfo;

        private float _currentTime = 0;

        public Animator(Skeleton skeleton)
        {
            _skeleton = skeleton;
            _animInfo = BoneAnimInfo.New();
        }

        public void Play(Animation animation)
        {
            CurrentAnimation = animation;
            _currentTime = 0;
        }

        public void Update(float deltaTime)
        {
            if (CurrentAnimation == null)
            {
                return;
            }

            _currentTime += deltaTime;
            if (_currentTime > CurrentAnimation.duration)
            {
                _currentTime = _currentTime - CurrentAnimation.duration;
            }

            this.SetToSetupPose();
            //this.Sample(_currentTime);
            this.ComputeWorldTransform();
        }

        private void SetToSetupPose()
        {
            for (int i = 0; i < _skeleton.Bones.Length; i++)
            {
                _animInfo.transformations[i] = _skeleton.Bones[i].transform;
            }
        }

        private void Sample(float time)
        {
            foreach (var channel in CurrentAnimation.channels)
            {
                int boneIdx = _skeleton.FindBoneIndex(channel.boneName);
                Matrix4x4 translation = Matrix4x4.CreateTranslation(channel.translationTimeline.Sample(time));
                Matrix4x4 rotation = Matrix4x4.CreateFromQuaternion(channel.rotationTimeline.Sample(time));
                Matrix4x4 scale = Matrix4x4.CreateScale(channel.scaleTimeline.Sample(time));

                _animInfo.transformations[boneIdx] = scale * rotation * translation;
            }
        }

        private void ComputeWorldTransform()
        {
            for (int boneIdx = 0; boneIdx < _skeleton.Bones.Length; boneIdx++)
            {
                ref var bone = ref _skeleton.Bones[boneIdx];
                if (bone.parentIndex != -1)
                {
                    _animInfo.transformations[boneIdx] *= _animInfo.transformations[bone.parentIndex];
                }
            }

            for (int boneIdx = 0; boneIdx < _skeleton.Bones.Length; boneIdx++)
            {
                _animInfo.transformations[boneIdx] = Matrix4x4.Transpose(_skeleton.Bones[boneIdx].offset * _animInfo.transformations[boneIdx] * _skeleton.InverseRootTransform);
            }
        }

        public BoneAnimInfo GetBoneTransformationData()
        {
            return _animInfo;
        }
    }
}