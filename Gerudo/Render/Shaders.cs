namespace Gerudo
{
    public static class Shaders
    {
        public const string COLOR_VERT = @"
            #version 450

            layout(set = 0, binding = 0) uniform PerFrameBuffer
            {
                mat4 View;
                mat4 Projection;
            };

            layout(set = 1, binding = 0) uniform PerDrawBuffer
            {
                mat4 World;
            };

            layout(location = 0) in vec3 aPosition;
            layout(location = 1) in vec2 aTexcoord;
            layout(location = 2) in vec3 aNormal;
            layout(location = 3) in vec4 aColor;

            layout(location = 0) out vec2 vUV;
            layout(location = 1) out vec3 vNormal;
            layout(location = 2) out vec4 vColor;

            void main()
            {
                gl_Position = Projection * View * World * vec4(aPosition, 1);
                vUV = aTexcoord;
                vNormal = (World * vec4(aNormal, 1)).xyz;
                vColor = aColor;
            }";

        public const string COLOR_FRAG = @"
            #version 450

            layout(set = 0, binding = 0) uniform PerFrameBuffer
            {
                mat4 View;
                mat4 Projection;
                vec4 LightColor;
                vec4 LightDirection;
            };

            layout(set = 2, binding = 0) uniform texture2D MainTexture;
            layout(set = 2, binding = 1) uniform sampler MainSampler;

            layout(location = 0) in vec2 vUV;
            layout(location = 1) in vec3 vNormal;
            layout(location = 2) in vec4 vColor;
            layout(location = 0) out vec4 fragColor;

            void main()
            {
                vec3 normal = normalize(vNormal);
                float NdotL = max(dot(normal, LightDirection.xyz), 0);
                vec4 mainColor = texture(sampler2D(MainTexture, MainSampler), vUV);
                fragColor = mainColor * vColor * LightColor * NdotL;
            }";

        public const string ANIMATED_VERT = @"
            #version 450

            layout(set = 0, binding = 0) uniform PerFrameBuffer
            {
                mat4 View;
                mat4 Projection;
                vec4 LightColor;
                vec4 LightDirection;
            };

            layout(set = 1, binding = 0) uniform PerDrawBuffer
            {
                mat4 World;
            };

            layout(set = 3, binding = 0) uniform BonesBuffer
            {
                mat4 BoneTransformations[64];
            };

            layout(location = 0) in vec3 aPosition;
            layout(location = 1) in vec2 aTexcoord;
            layout(location = 2) in vec3 aNormal;
            layout(location = 3) in vec4 aColor;
            layout(location = 4) in uvec4 aBoneIndices;
            layout(location = 5) in vec4 aBoneWeights;

            layout(location = 0) out vec2 vUV;
            layout(location = 1) out vec3 vNormal;
            layout(location = 2) out vec4 vColor;

            void main()
            {
                mat4 boneTransformation = BoneTransformations[aBoneIndices.x]  * aBoneWeights.x;
                boneTransformation += BoneTransformations[aBoneIndices.y]  * aBoneWeights.y;
                boneTransformation += BoneTransformations[aBoneIndices.z]  * aBoneWeights.z;
                boneTransformation += BoneTransformations[aBoneIndices.w]  * aBoneWeights.w;

                gl_Position = Projection * View * World * boneTransformation * vec4(aPosition, 1);
                vUV = aTexcoord;
                vNormal = (World * vec4(aNormal, 1)).xyz;
                vColor = aColor;
            }";
    }
}