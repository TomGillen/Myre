using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.UI.InputDevices;
using Microsoft.Xna.Framework.Input;

namespace Myre.UI.Gestures
{
    public class ButtonReleased
        : Gesture<GamepadDevice>
    {
        public Buttons Button { get; private set; }

        public ButtonReleased(Buttons button)
            : base(false)
        {
            Button = button;
            BlockedInputs.Add((int)Button);
        }

        public override bool Test(GamepadDevice device)
        {
            return device.IsButtonNewlyUp(Button);
        }
    }
}