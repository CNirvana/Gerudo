using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Gerudo
{
    public class ModelLoader : IAssetLoader
    {
        public IAsset Load(string path)
        {
            var importer = new Assimp.AssimpContext();
            var importFlags = Assimp.PostProcessSteps.Triangulate
                | Assimp.PostProcessSteps.GenerateNormals
                | Assimp.PostProcessSteps.FlipWindingOrder
                | Assimp.PostProcessSteps.FlipUVs
                | Assimp.PostProcessSteps.LimitBoneWeights;
            var scene = importer.ImportFile(path, importFlags);

            if (scene.HasMeshes == false)
            {
                return null;
            }

            var model = new Model();
            for (int i = 0; i < scene.MeshCount; i++)
            {
                var mesh = ReadMesh(scene, i);
                model.meshes.Add(mesh);
            }

            for (int i = 0; i < scene.AnimationCount; i++)
            {
                var animation = ReadAnimation(scene.Animations[i]);
                model.animations.Add(animation);
            }

            return model;
        }

        private Mesh ReadMesh(Assimp.Scene scene, int meshIndex)
        {
            var meshData = scene.Meshes[meshIndex];
            if (meshData.HasBones)
            {
                return ReadSkeletalMesh(scene, meshData);
            }
            else
            {
                return ReadStaticMesh(meshData);
            }
        }

        private SkeletalMesh ReadSkeletalMesh(Assimp.Scene scene, Assimp.Mesh meshData)
        {
            var meshDataUVs = meshData.HasTextureCoords(0) ? meshData.TextureCoordinateChannels[0] : null;
            var defaultUV = new Assimp.Vector3D(0, 0, 0);
            var meshDataColors = meshData.HasVertexColors(0) ? meshData.VertexColorChannels[0] : null;
            var defaultColor = new Assimp.Color4D(1, 1, 1, 1);

            SkeletalVertex[] vertices = new SkeletalVertex[meshData.VertexCount];
            for (int i = 0; i < meshData.VertexCount; i++)
            {
                ref var vertex = ref vertices[i];

                var position = meshData.Vertices[i];
                vertex.position = new Vector3(position.X, position.Y, position.Z);

                var texcoord = meshDataUVs != null ? meshDataUVs[i] : defaultUV;
                vertex.texcoord = new Vector2(texcoord.X, texcoord.Y);

                var color = meshDataColors != null ? meshDataColors[i] : defaultColor;
                vertex.color = new Veldrid.RgbaFloat(color.R, color.G, color.B, color.A);

                var normal = meshData.Normals[i];
                vertex.normal = new Vector3(normal.X, normal.Y, normal.Z);

                vertex.boneIndices = new UInt4(0, 0, 0, 0);
                vertex.boneWeights = Vector4.Zero;
            }

            var skeleton = ReadSkeleton(scene, meshData);
            ExtractBoneWeightForVertices(vertices, meshData, skeleton);

            var shortIndices = meshData.GetShortIndices();
            ushort[] indices = new ushort[shortIndices.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = (ushort)shortIndices[i];
            }

            return SkeletalMesh.Create(vertices, indices, skeleton);
        }

        private void ExtractBoneWeightForVertices(SkeletalVertex[] vertices, Assimp.Mesh meshData, Skeleton skeleton)
        {
            for (int i = 0; i < meshData.BoneCount; i++)
            {
                var boneData = meshData.Bones[i];
                var boneIndex = Array.FindIndex<Bone>(skeleton.Bones, (value) => value.name == boneData.Name);

                foreach (var weight in boneData.VertexWeights)
                {
                    ref var vertex = ref vertices[weight.VertexID];
                    if (vertex.boneWeights.X == 0)
                    {
                        vertex.boneIndices.X = (uint)boneIndex;
                        vertex.boneWeights.X = weight.Weight;
                    }
                    else if (vertex.boneWeights.Y == 0)
                    {
                        vertex.boneIndices.Y = (uint)boneIndex;
                        vertex.boneWeights.Y = weight.Weight;
                    }
                    else if (vertex.boneWeights.Z == 0)
                    {
                        vertex.boneIndices.Z = (uint)boneIndex;
                        vertex.boneWeights.Z = weight.Weight;
                    }
                    else if (vertex.boneWeights.W == 0)
                    {
                        vertex.boneIndices.W = (uint)boneIndex;
                        vertex.boneWeights.W = weight.Weight;
                    }
                }
            }
        }

        private Skeleton ReadSkeleton(Assimp.Scene scene, Assimp.Mesh mesh)
        {
            var skeleton = new Skeleton();
            var rootTransform = scene.RootNode.Transform;
            rootTransform.Inverse();
            skeleton.InverseRootTransform = ToSystemMatrix(rootTransform);

            List<NodePair> allNodes = new List<NodePair>();
            RetrieveNodeHierarchy(allNodes, scene.RootNode);

            var meshNode = allNodes.Find((value) => {
                if (value.node.HasMeshes)
                {
                    foreach (var meshIdx in value.node.MeshIndices)
                    {
                        if (scene.Meshes[meshIdx] == mesh)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }).node;

            foreach (var bone in mesh.Bones)
            {
                var pair = allNodes.Find((value) => value.node.Name == bone.Name);
                if (pair.necessary)
                {
                    continue;
                }

                pair.necessary = true;

                var curNode = pair.node.Parent;
                while(curNode != meshNode && curNode.Parent != null)
                {
                    pair = allNodes.Find((value) => value.node.Name == curNode.Name);
                    if (!pair.necessary)
                    {
                        pair.necessary = true;
                        curNode = pair.node.Parent;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            allNodes.RemoveAll((value) => !value.necessary);

            skeleton.BoneNameToIdx = new Dictionary<string, int>();
            skeleton.Bones = new Bone[allNodes.Count];
            for (int i = 0; i < allNodes.Count; i++)
            {
                var node = allNodes[i].node;
                ref var bone = ref skeleton.Bones[i];
                bone.name = node.Name;
                bone.transform = ToSystemMatrix(node.Transform);
                bone.parentIndex = allNodes.FindIndex((value) => node.Parent == value.node);
                bone.offset = Matrix4x4.Identity;

                var meshBone = mesh.Bones.Find((value) => value.Name == node.Name);
                if (meshBone != null)
                {
                    bone.offset = ToSystemMatrix(meshBone.OffsetMatrix);
                }

                skeleton.BoneNameToIdx.Add(bone.name, i);
            }

            return skeleton;
        }

        private void RetrieveNodeHierarchy(List<NodePair> allNodes, Assimp.Node node)
        {
            allNodes.Add(new NodePair(node, false));
            foreach (var child in node.Children)
            {
                RetrieveNodeHierarchy(allNodes, child);
            }
        }

        private Animation ReadAnimation(Assimp.Animation animationInfo)
        {
            var animation = new Animation();
            animation.name = animationInfo.Name;
            animation.duration = (float)(animationInfo.DurationInTicks / animationInfo.TicksPerSecond);

            foreach (var nodeAnimationChannel in animationInfo.NodeAnimationChannels)
            {
                var channel = new Animation.Channel();
                channel.boneName = nodeAnimationChannel.NodeName;

                if (nodeAnimationChannel.HasPositionKeys)
                {
                    channel.translationTimeline = ReadPositionTimeline(nodeAnimationChannel.PositionKeys);
                }

                if (nodeAnimationChannel.HasRotationKeys)
                {
                    channel.rotationTimeline = ReadRotationTimeline(nodeAnimationChannel.RotationKeys);
                }

                if (nodeAnimationChannel.HasScalingKeys)
                {
                    channel.scaleTimeline = ReadScaleTimeline(nodeAnimationChannel.ScalingKeys);
                }

                animation.channels.Add(channel);
            }

            return animation;
        }

        private TranslationTimeline ReadPositionTimeline(List<Assimp.VectorKey> positionKeys)
        {
            var timeline = new TranslationTimeline();
            timeline.frames = new Timeline<Vector3>.Keyframe[positionKeys.Count];
            for (int i = 0; i < positionKeys.Count; i++)
            {
                timeline.frames[i].time = (float)positionKeys[i].Time;
                timeline.frames[i].value = ToSystemVector3(positionKeys[i].Value);
            }

            return timeline;
        }

        private RotationTimeline ReadRotationTimeline(List<Assimp.QuaternionKey> rotationKeys)
        {
            var timeline = new RotationTimeline();
            timeline.frames = new Timeline<Quaternion>.Keyframe[rotationKeys.Count];
            for (int i = 0; i < rotationKeys.Count; i++)
            {
                timeline.frames[i].time = (float)rotationKeys[i].Time;
                timeline.frames[i].value = ToSystemQuaternion(rotationKeys[i].Value);
            }

            return timeline;
        }

        private ScaleTimeline ReadScaleTimeline(List<Assimp.VectorKey> scaleKeys)
        {
            var timeline = new ScaleTimeline();
            timeline.frames = new Timeline<Vector3>.Keyframe[scaleKeys.Count];
            for (int i = 0; i < scaleKeys.Count; i++)
            {
                timeline.frames[i].time = (float)scaleKeys[i].Time;
                timeline.frames[i].value = ToSystemVector3(scaleKeys[i].Value);
            }

            return timeline;
        }

        private StaticMesh ReadStaticMesh(Assimp.Mesh meshData)
        {
            var meshDataUVs = meshData.HasTextureCoords(0) ? meshData.TextureCoordinateChannels[0] : null;
            var defaultUV = new Assimp.Vector3D(0, 0, 0);
            var meshDataColors = meshData.HasVertexColors(0) ? meshData.VertexColorChannels[0] : null;
            var defaultColor = new Assimp.Color4D(1, 1, 1, 1);

            Vertex[] vertices = new Vertex[meshData.VertexCount];
            for (int i = 0; i < meshData.VertexCount; i++)
            {
                ref var vertex = ref vertices[i];

                var position = meshData.Vertices[i];
                vertex.position = new Vector3(position.X, position.Y, position.Z);

                var texcoord = meshDataUVs != null ? meshDataUVs[i] : defaultUV;
                vertex.texcoord = new Vector2(texcoord.X, texcoord.Y);

                var color = meshDataColors != null ? meshDataColors[i] : defaultColor;
                vertex.color = new Veldrid.RgbaFloat(color.R, color.G, color.B, color.A);

                var normal = meshData.Normals[i];
                vertex.normal = new Vector3(normal.X, normal.Y, normal.Z);
            }

            var shortIndices = meshData.GetShortIndices();
            ushort[] indices = new ushort[shortIndices.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = (ushort)shortIndices[i];
            }

            return StaticMesh.Create(vertices, indices);
        }

        public unsafe static Matrix4x4 ToSystemMatrix(Assimp.Matrix4x4 matrix)
        {
            return Unsafe.Read<Matrix4x4>(&matrix);
        }

        private Vector3 ToSystemVector3(Assimp.Vector3D vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        private Quaternion ToSystemQuaternion(Assimp.Quaternion quat)
        {
            return new Quaternion(quat.X, quat.Y, quat.Z, quat.W);
        }

        private class NodePair
        {
            public Assimp.Node node;

            public bool necessary;

            public NodePair(Assimp.Node node, bool necessary)
            {
                this.node = node;
                this.necessary = necessary;
            }
        }
    }
}