using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.UI.InputDevices;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Myre.UI.Gestures
{
    public class ThumbstickMoved
        : Gesture<GamepadDevice>
    {
        public Side Thumbstick { get; private set; }

        public ThumbstickMoved(Side thumbstick)
            : base(false)
        {
            Thumbstick = thumbstick;
            BlockedInputs.Add((int)Thumbstick + 25);//Enum.GetValues(typeof(Buttons)).Length);
        }

        public override bool Test(GamepadDevice device)
        {
            if (Thumbstick == Side.Left)
                return device.LeftThumbstickMovement != Vector2.Zero;
            else
                return device.RightThumbstickMovement != Vector2.Zero;
        }
    }
}