using System.Numerics;
using Gerudo;
using Gerudo.ECS;
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

        private World _world;

        private SystemManager _systemManager;

        protected override void Initialize()
        {
            var model = AssetManager.LoadAsset<Model>("Assets/Drone/Drone.fbx");
            var texture = AssetManager.LoadAsset<Texture2D>("Assets/Drone/Drone_diff.jpg");

            var renderer = new Renderer
            {
                Model = model,
                //Mesh = mesh,
                Transform = new Transform() { Scale = Vector3.One * 0.01f},
                Material = new Material(texture)
            };

            Scene.AddRenderer(renderer);

            var quadModel = new Model();
            quadModel.meshes.Add(CreateQuad());
            var quadRenderer = new Renderer
            {
                Model = quadModel,
                Transform = new Transform() { Position = Vector3.UnitX * 5f },
                Material = new Material(texture)
            };

            Scene.AddRenderer(quadRenderer);

            _world = new World();
            _systemManager = new SystemManager(_world);
            _systemManager.Add(new UpdateSystem()).Init();
        }

        protected override void Update(float deltaTime)
        {
            _systemManager.Update();

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

            if (Input.Keyboard.GetKeyDown(Key.Escape))
            {
                Exit();
            }
        }

        protected override void OnGUI()
        {
        }

        protected override void Cleanup()
        {
            if (_systemManager != null)
            {
                _systemManager.Destroy();
                _systemManager = null;
            }
            if (_world != null)
            {
                _world.Destroy();
                _world = null;
            }
        }

        private Mesh CreateQuad()
        {
            Vertex[] vertices = new Vertex[]
            {
                new Vertex(new Vector3(-0.5f, -0.5f, 0), new Vector2(0, 1), new Vector3(0, 0, 1), RgbaFloat.White),
                new Vertex(new Vector3(+0.5f, -0.5f, 0), new Vector2(1, 1), new Vector3(0, 0, 1), RgbaFloat.White),
                new Vertex(new Vector3(-0.5f, +0.5f, 0), new Vector2(0, 0), new Vector3(0, 0, 1), RgbaFloat.White),
                new Vertex(new Vector3(+0.5f, +0.5f, 0), new Vector2(1, 0), new Vector3(0, 0, 1), RgbaFloat.White),
            };
            ushort[] indices = { 0, 2, 1, 2, 3, 1 };

            return Mesh.Create(vertices, indices);
        }

        struct TagComponent :IComponent
        {
        }

        class UpdateSystem : IInitSystem, IUpdateSystem, IDestroySystem
        {
            public void Init(SystemManager systems)
            {
                var world = systems.GetWorld();
                var entity = world.CreateEntity();
                entity.AddComp<TagComponent>();
            }

            public void Update(SystemManager systems)
            {
                var world = systems.GetWorld();
                var filter = world.Filter<TagComponent>().End();

                foreach (var entity in filter)
                {
                }
            }

            public void Destroy(SystemManager systems)
            {
            }
        }
    }
}