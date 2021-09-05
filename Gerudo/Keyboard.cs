using Veldrid;
using Veldrid.Sdl2;

namespace Gerudo
{
    public class Keyboard
    {
        private bool[] _lastFramePressedKey = new bool[(int)Key.LastKey];

        private bool[] _currentFramePressedKey = new bool[(int)Key.LastKey];

        public bool GetKeyDown(Key key)
        {
            return !_lastFramePressedKey[(int)key] && _currentFramePressedKey[(int)key];
        }

        public bool GetKey(Key key)
        {
            return _currentFramePressedKey[(int)key];
        }

        public bool GetKeyUp(Key key)
        {
            return _lastFramePressedKey[(int)key] && !_currentFramePressedKey[(int)key];
        }

        internal void Update(InputSnapshot snapshot)
        {
            for (int i = 0; i < _lastFramePressedKey.Length; i++)
            {
                _lastFramePressedKey[i] = _currentFramePressedKey[i];
            }

            foreach (var keyEvent in snapshot.KeyEvents)
            {
                _currentFramePressedKey[(int)keyEvent.Key] = keyEvent.Down;
            }
        }
    }
}