using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace RemoteJoystickWeb
{
    class KeyboardEmulator
    {

        public void KeyPress(string name)
        {
            if (ParseKey(name, out VirtualKeyCode res))
            {
                sim.Keyboard.KeyPress(res);
            }
        }

        public void KeyUp(string name)
        {
            if (ParseKey(name, out VirtualKeyCode res))
            {
                sim.Keyboard.KeyUp(res);
            }
        }

        public void KeyDown(string name)
        {
            if (ParseKey(name, out VirtualKeyCode res))
            {
                sim.Keyboard.KeyDown(res);
            }
        }

        private bool ParseKey(string name, out VirtualKeyCode res)
        {
            name = name.ToUpper();
            if (Enum.TryParse(name, out res) || Enum.TryParse("VK_" + name, out res))
            {
                return true;
            }
            return false;
        }

        private static InputSimulator sim = new InputSimulator();
    }
}
