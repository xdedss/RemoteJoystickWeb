using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vJoyInterfaceWrap;

namespace RemoteJoystickWeb
{
    class JoystickEmulator
    {
        private uint id;
        private vJoy joystick;

        public JoystickEmulator(uint id)
        {
            this.id = id;
            this.joystick = new vJoy();
            // Get the driver attributes (Vendor ID, Product ID, Version Number)
            if (!joystick.vJoyEnabled())
            {
                Console.WriteLine("vJoy driver not enabled: Failed Getting vJoy attributes.\n");
                throw new Exception("vJoy driver not enabled: Failed Getting vJoy attributes.");
            }
            else
            {
                Console.WriteLine("Vendor: {0} | Product :{1} | Version Number:{2}  ",
                    joystick.GetvJoyManufacturerString(),
                    joystick.GetvJoyProductString(),
                    joystick.GetvJoySerialNumberString());
            }
            
            VjdStat status;
            string prt;
            status = joystick.GetVJDStatus(id);
            if ((status == VjdStat.VJD_STAT_OWN) ||
            ((status == VjdStat.VJD_STAT_FREE) && (!joystick.AcquireVJD(id))))
            {
                prt = String.Format("Failed to acquire vJoy device number {0}.", id);
                throw new Exception(prt);
            }
            else
            {
                prt = String.Format("Acquired: vJoy device number {0}.", id);
                Console.WriteLine(prt);
            }
        }

        public void SetAxis(string name, float value)
        {
            name = name.ToUpper();
            if (axisCache.ContainsKey(name))
            {
                joystick.SetAxisf(value, id, axisCache[name]);
            }
            else
            {
                HID_USAGES axis;
                if (Enum.TryParse(name, out axis) || Enum.TryParse("HID_USAGE_" + name, out axis))
                {
                    axisCache.Add(name, axis);
                    joystick.SetAxisf(value, id, axis);
                }
            }
        }

        public void SetButton(uint nbtn, bool value)
        {
            joystick.SetBtn(value, id, nbtn);
        }

        private static Dictionary<string, HID_USAGES> axisCache = new Dictionary<string, HID_USAGES>();
    }
}
