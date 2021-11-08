using System.Numerics;
using Gerudo;
using ImGuiNET;
using Veldrid;

namespace Sandbox
{
    public class SandboxApp : Engine
    {
        private float _moveSpeed = 1.0f;

        private float _fastMoveSpeed = 10f;

        private float _rotateSpeed = 180.0f;

        private float _yaw;

        private float _pitch;

        private SkeletalMesh _mesh;

        private Animator _animator;

        private World _world;

        protected override void Initialize()
        {
            var model = AssetDatabase.LoadAsset<Model>("Assets/vampire/dancing_vampire.dae");
            var texture = AssetDatabase.LoadAsset<Texture2D>("Assets/vampire/textures/Vampire_diffuse.png");

            var renderer = new Renderer
            {
                Model = model,
                //Mesh = mesh,
                Transform = new Transform() { Scale = Vector3.One * 0.0001f},
                Material = new Material(texture)
            };

            _mesh = model.meshes[0] as SkeletalMesh;
            _animator = new Animator(_mesh.Skeleton);
            _animator.Play(model.animations[0]);

            Scene.AddRenderer(renderer);

            // var quadModel = new Model();
            // quadModel.meshes.Add(CreateQuad());
            // var quadRenderer = new Renderer
            // {
            //     Model = quadModel,
            //     Transform = new Transform() { Position = Vector3.UnitX * 5f },
            //     Material = new Material(texture)
            // };

            // Scene.AddRenderer(quadRenderer);

            _world = new World(new WorldConfig());
            _world.Initialize();
        }

        protected override void Update(float deltaTime)
        {
            _animator.Update(deltaTime);
            _mesh.boneTransformationData = _animator.GetBoneTransformationData();

            var camera = Scene.Camera;

            if (Input.Mouse.GetButton(MouseButton.Right))
            {
                _yaw -= Input.Mouse.MouseDelta.X * deltaTime * _rotateSpeed;
                _pitch -= Input.Mouse.MouseDelta.Y * deltaTime * _rotateSpeed;
                _pitch = MathHelper.Clamp(_pitch, -80f, 80f);

                Window.CursorVisible = false;
            }
            else
            {
                Window.CursorVisible = true;
            }

            camera.Transform.Rotation = Quaternion.CreateFromYawPitchRoll(_yaw * MathHelper.DEG2RAD, _pitch * MathHelper.DEG2RAD, 0);

            var forward = -camera.Transform.UnitZ;
            var right = camera.Transform.UnitX;
            var up = camera.Transform.UnitY;
            var moveSpeed = Input.Keyboard.GetKey(Key.LShift) ? _fastMoveSpeed : _moveSpeed;

            var position = camera.Transform.Position;
            if (Input.Keyboard.GetKey(Key.W))
            {
                position += forward * moveSpeed * deltaTime;
            }
            if (Input.Keyboard.GetKey(Key.S))
            {
                position -= forward * moveSpeed * deltaTime;
            }
            if (Input.Keyboard.GetKey(Key.D))
            {
                position += right * moveSpeed * deltaTime;
            }
            if (Input.Keyboard.GetKey(Key.A))
            {
                position -= right * moveSpeed * deltaTime;
            }
            if (Input.Keyboard.GetKey(Key.E))
            {
                position += up * moveSpeed * deltaTime;
            }
            if (Input.Keyboard.GetKey(Key.Q))
            {
                position -= up * moveSpeed * deltaTime;
            }

            camera.Transform.Position = position;

            _world.Update(deltaTime);

            if (Input.Keyboard.GetKeyDown(Key.Escape))
            {
                Exit();
            }
        }

        protected override void OnGUI()
        {
            ImGui.StyleColorsClassic();
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Open"))
                    {
                        
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMainMenuBar();
            }
        }

        protected override void Cleanup()
        {
            _world.Destroy();
        }

        private StaticMesh CreateQuad()
        {
            Vertex[] vertices = new Vertex[]
            {
                new Vertex(new Vector3(-0.5f, -0.5f, 0), new Vector2(0, 1), new Vector3(0, 0, 1), RgbaFloat.White),
                new Vertex(new Vector3(+0.5f, -0.5f, 0), new Vector2(1, 1), new Vector3(0, 0, 1), RgbaFloat.White),
                new Vertex(new Vector3(-0.5f, +0.5f, 0), new Vector2(0, 0), new Vector3(0, 0, 1), RgbaFloat.White),
                new Vertex(new Vector3(+0.5f, +0.5f, 0), new Vector2(1, 0), new Vector3(0, 0, 1), RgbaFloat.White),
            };
            ushort[] indices = { 0, 2, 1, 2, 3, 1 };

            return StaticMesh.Create(vertices, indices);
        }

        public struct WeaponComp : IComponent
        {
            public int ammo;
        }

        public class WeaponUpdateSystem : IInitSystem, IUpdateSystem
        {
            private EntityMask _mask;

            public void Init(World world)
            {
                _mask = new EntityMask().Include<WeaponComp>();
            }

            public void Update(World world, float deltaTime)
            {
                var filter = world.GetFilter(_mask);
                foreach (var entity in filter)
                {
                    ref var weapon = ref world.GetComp<WeaponComp>(entity);
                    weapon.ammo++;
                }
            }
        }
    }
}