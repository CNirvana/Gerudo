using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace Gerudo
{
    public class Mouse
    {
        public Vector2 MousePosition { get; private set; }

        public Vector2 MouseDelta { get; private set; }

        public float WheelDelta { get; private set; }

        private bool[] _lastFramePressedButton = new bool[(int)MouseButton.LastButton];

        private bool[] _currentFramePressedButton= new bool[(int)MouseButton.LastButton];

        public bool GetButtonDown(MouseButton button)
        {
            return !_lastFramePressedButton[(int)button] && _currentFramePressedButton[(int)button];
        }

        public bool GetButton(MouseButton button)
        {
            return _currentFramePressedButton[(int)button];
        }

        public bool GetButtonUp(MouseButton button)
        {
            return _lastFramePressedButton[(int)button] && !_currentFramePressedButton[(int)button];
        }

        public void Update(InputSnapshot snapshot, Sdl2Window window)
        {
            MousePosition = snapshot.MousePosition;
            WheelDelta = snapshot.WheelDelta;
            MouseDelta = window.MouseDelta;

            for (int i = 0; i < _lastFramePressedButton.Length; i++)
            {
                _lastFramePressedButton[i] = _currentFramePressedButton[i];
            }

            foreach (var mouseEvent in snapshot.MouseEvents)
            {
                _currentFramePressedButton[(int)mouseEvent.MouseButton] = mouseEvent.Down;
            }
        }
    }
}