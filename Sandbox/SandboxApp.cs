using System.Numerics;
using Gerudo;
using Veldrid;

namespace Sandbox
{
    public class SandboxApp : Engine
    {
        private Renderer _renderer;

        private float _moveSpeed = 5.0f;

        private float _rotateSpeed = 180.0f;

        private float _yaw;

        private float _pitch;

        protected override void Initialize()
        {
           Vertex[] vertices = new Vertex[]
            {
                // Top
                new Vertex(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(0, 0), new Vector3(0, 1, 0), RgbaFloat.Red),
                new Vertex(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(1, 0), new Vector3(0, 1, 0), RgbaFloat.Red),
                new Vertex(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(1, 1), new Vector3(0, 1, 0), RgbaFloat.Red),
                new Vertex(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(0, 1), new Vector3(0, 1, 0), RgbaFloat.Red),
                // Bottom                                                             
                new Vertex(new Vector3(-0.5f,-0.5f, +0.5f),  new Vector2(0, 0), new Vector3(0, -1, 0), RgbaFloat.Cyan),
                new Vertex(new Vector3(+0.5f,-0.5f, +0.5f),  new Vector2(1, 0), new Vector3(0, -1, 0), RgbaFloat.Cyan),
                new Vertex(new Vector3(+0.5f,-0.5f, -0.5f),  new Vector2(1, 1), new Vector3(0, -1, 0), RgbaFloat.Cyan),
                new Vertex(new Vector3(-0.5f,-0.5f, -0.5f),  new Vector2(0, 1), new Vector3(0, -1, 0), RgbaFloat.Cyan),
                // Left                                                               
                new Vertex(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(0, 0), new Vector3(-1, 0, 0), RgbaFloat.White),
                new Vertex(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(1, 0), new Vector3(-1, 0, 0), RgbaFloat.White),
                new Vertex(new Vector3(-0.5f, -0.5f, +0.5f), new Vector2(1, 1), new Vector3(-1, 0, 0), RgbaFloat.White),
                new Vertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(0, 1), new Vector3(-1, 0, 0), RgbaFloat.White),
                // Right                                                              
                new Vertex(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(0, 0), new Vector3(1, 0, 0), RgbaFloat.Green),
                new Vertex(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(1, 0), new Vector3(1, 0, 0), RgbaFloat.Green),
                new Vertex(new Vector3(+0.5f, -0.5f, -0.5f), new Vector2(1, 1), new Vector3(1, 0, 0), RgbaFloat.Green),
                new Vertex(new Vector3(+0.5f, -0.5f, +0.5f), new Vector2(0, 1), new Vector3(1, 0, 0), RgbaFloat.Green),
                // Back                                                               
                new Vertex(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(0, 0), new Vector3(0, 0, -1), RgbaFloat.Yellow),
                new Vertex(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(1, 0), new Vector3(0, 0, -1), RgbaFloat.Yellow),
                new Vertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(1, 1), new Vector3(0, 0, -1), RgbaFloat.Yellow),
                new Vertex(new Vector3(+0.5f, -0.5f, -0.5f), new Vector2(0, 1), new Vector3(0, 0, -1), RgbaFloat.Yellow),
                // Front                                                              
                new Vertex(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(0, 0), new Vector3(0, 0, 1), RgbaFloat.Blue),
                new Vertex(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(1, 0), new Vector3(0, 0, 1), RgbaFloat.Blue),
                new Vertex(new Vector3(+0.5f, -0.5f, +0.5f), new Vector2(1, 1), new Vector3(0, 0, 1), RgbaFloat.Blue),
                new Vertex(new Vector3(-0.5f, -0.5f, +0.5f), new Vector2(0, 1), new Vector3(0, 0, 1), RgbaFloat.Blue),
            };
            ushort[] indices = {
                0,1,2, 0,2,3,
                4,5,6, 4,6,7,
                8,9,10, 8,10,11,
                12,13,14, 12,14,15,
                16,17,18, 16,18,19,
                20,21,22, 20,22,23,
            };

            var mesh = Mesh.Create(vertices, indices);
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
                _pitch = MathHelper.Clamp(_pitch, -65f, 65f);

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

            var position = camera.Transform.Position;
            if (Input.Keyboard.GetKey(Key.W))
            {
                position += forward * _moveSpeed * deltaTime;
            }
            if (Input.Keyboard.GetKey(Key.S))
            {
                position -= forward * _moveSpeed * deltaTime;
            }
            if (Input.Keyboard.GetKey(Key.D))
            {
                position += right * _moveSpeed * deltaTime;
            }
            if (Input.Keyboard.GetKey(Key.A))
            {
                position -= right * _moveSpeed * deltaTime;
            }
            if (Input.Keyboard.GetKey(Key.E))
            {
                position += up * _moveSpeed * deltaTime;
            }
            if (Input.Keyboard.GetKey(Key.Q))
            {
                position -= up * _moveSpeed * deltaTime;
            }

            camera.Transform.Position = position;
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