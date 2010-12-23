using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.UI.InputDevices;
using Microsoft.Xna.Framework.Input;

namespace Myre.UI.Gestures
{
    public class CharactersEntered
        : Gesture<KeyboardDevice>
    {
        public CharactersEntered()
            : base(false)
        {
            this.BlockedInputs.Add(-1);
        }

        public override bool Test(KeyboardDevice device)
        {
            return device.Characters.Count > 0;
        }
    }
}