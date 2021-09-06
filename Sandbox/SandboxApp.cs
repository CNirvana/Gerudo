using System.Numerics;
using Gerudo;
using Veldrid;

namespace Sandbox
{
    public class SandboxApp : Engine
    {
        private Renderer _renderer;

        private float _moveSpeed = 1.0f;

        private float _fastMoveSpeed = 10f;

        private float _rotateSpeed = 180.0f;

        private float _yaw;

        private float _pitch;

        protected override void Initialize()
        {
            var model = AssetManager.LoadAsset<Model>("Assets/viking_room.obj");
            var texture = AssetManager.LoadAsset<Texture2D>("Assets/viking_room.png");

            _renderer = new Renderer
            {
                Model = model,
                //Mesh = mesh,
                Transform = new Transform(),
                Material = new Material(texture)
            };

            Scene.AddRenderer(_renderer);

            var quadModel = new Model();
            quadModel.meshes.Add(CreateQuad());
            var quadRenderer = new Renderer
            {
                Model = quadModel,
                Transform = new Transform() { Position = Vector3.UnitX * 5f },
                Material = new Material(texture)
            };

            Scene.AddRenderer(quadRenderer);
        }

        protected override void Update(float deltaTime)
        {
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

        protected override void Cleanup()
        {
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
    }
}