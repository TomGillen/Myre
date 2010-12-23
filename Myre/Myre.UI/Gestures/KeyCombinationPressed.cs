using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.UI.InputDevices;
using Microsoft.Xna.Framework.Input;

namespace Myre.UI.Gestures
{
    public class KeyCombinationPressed
        : Gesture<KeyboardDevice>
    {
        public Keys[] Keys { get; private set; }
        public Keys[] Modifiers { get; private set; }

        public KeyCombinationPressed(params Keys[] keys)
            : this(keys, new Keys[0])
        {
        }

        public KeyCombinationPressed(Keys key, params Keys[] modifiers)
            : this(new Keys[] { key }, modifiers)
        {
        }

        public KeyCombinationPressed(Keys[] keys, params Keys[] modifiers)
            : base(false)
        {
            this.Keys = keys;
            this.Modifiers = modifiers;

            for (int i = 0; i < keys.Length; i++)
                BlockedInputs.Add((int)keys[i]);
        }

        public override bool Test(KeyboardDevice device)
        {
            // modifiers are pressed,
            // not all the main keys were pressed,
            // but they are all pressed now
            return AllKeysArePressed(device, Modifiers)
                && !AllKeysWerePressed(device, Keys)
                && AllKeysArePressed(device, Keys);
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
