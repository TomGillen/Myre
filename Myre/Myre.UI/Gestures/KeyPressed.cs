using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.UI.InputDevices;
using Microsoft.Xna.Framework.Input;

namespace Myre.UI.Gestures
{
    public class KeyPressed
        : Gesture<KeyboardDevice>
    {
        public Keys Key { get; private set; }

        public KeyPressed(Keys key)
            : base(false)
        {
            this.Key = key;
            this.BlockedInputs.Add((int)key);
        }

        public override bool Test(KeyboardDevice device)
        {
            return device.IsKeyNewlyDown(Key);
        }
    }
}
