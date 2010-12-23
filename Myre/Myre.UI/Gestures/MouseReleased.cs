using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.UI.InputDevices;

namespace Myre.UI.Gestures
{
    public class MouseReleased
        : Gesture<MouseDevice>
    {
        public MouseButtons Button { get; private set; }

        public MouseReleased(MouseButtons button)
            : base(false)
        {
            Button = button;
            base.BlockedInputs.Add((int)Button);
        }

        public override bool Test(MouseDevice device)
        {
            return device.IsButtonNewlyUp(Button);
        }
    }
}