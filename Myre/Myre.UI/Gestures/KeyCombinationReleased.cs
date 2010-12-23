using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.UI.InputDevices;
using Microsoft.Xna.Framework.Input;

namespace Myre.UI.Gestures
{
    public class KeyCombinationReleased
        : Gesture<KeyboardDevice>
    {
        public Keys[] Keys { get; private set; }

        public KeyCombinationReleased(params Keys[] keys)
            : base(false)
        {
            this.Keys = keys;

            for (int i = 0; i < keys.Length; i++)
                BlockedInputs.Add((int)keys[i]);
        }

        public override bool Test(KeyboardDevice device)
        {
            return AllKeysWerePressed(device, Keys)
                && !AllKeysArePressed(device, Keys);
        }

        private bool AllKeysArePressed(KeyboardDevice device, Keys[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (!device.IsKeyDown(keys[i]))
                    return false;
            }

            return true;
        }

        private bool AllKeysWerePressed(KeyboardDevice device, Keys[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (!device.WasKeyDown(keys[i]))
                    return false;
            }

            return true;
        }
    }
}
