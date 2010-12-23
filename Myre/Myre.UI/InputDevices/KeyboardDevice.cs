using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.ObjectModel;
using Myre.Extensions;

namespace Myre.UI.InputDevices
{
    public class KeyboardDevice
        : IInputDevice
    {
        private PlayerIndex player;
        private KeyboardState previousState;
        private KeyboardState currentState;
        private List<char> newCharacters;
        private List<char> characters;
        private List<int> blocked;
        private bool charactersBlocked;

        /// <summary>
        /// Gets the owner of this keyboard device.
        /// </summary>
        public InputActor Owner { get; set; }

        /// <summary>
        /// Gets a collection of chars which were entered in the previous frame.
        /// This only works on Windows. Use Guide.BeginShowKeyboardInput on Xbox or Windows Phone.
        /// </summary>
        public ReadOnlyCollection<char> Characters { get; private set; }

        public KeyboardDevice(PlayerIndex player)
            : this(player, IntPtr.Zero)
        {
        }

        public KeyboardDevice(PlayerIndex player, IntPtr windowHandle)
        {
            this.player = player;
            this.currentState = Keyboard.GetState(player);
            this.previousState = currentState;
            this.blocked = new List<int>();
            this.newCharacters = new List<char>();
            this.characters = new List<char>();
            this.Characters = new ReadOnlyCollection<char>(characters);

#if WINDOWS
            if (player == PlayerIndex.One)
            {
                if (windowHandle != IntPtr.Zero)
                    TextInput.Initialize(windowHandle);

                TextInput.CharEntered += (sender, e) =>
                    {
                        if (char.IsControl(e.Character))
                            return;

                        lock (newCharacters)
                            newCharacters.Add(e.Character);
                    };
            }
#endif
        }

        public void Update(GameTime gameTime)
        {
            previousState = currentState;
            currentState = Keyboard.GetState(player);

            lock (newCharacters)
            {
                characters.Clear();
                characters.AddRange(newCharacters);
                newCharacters.Clear();
            }
        }

        public void Evaluate(GameTime gameTime, Control focused, UserInterface ui)
        {
            var type = typeof(KeyboardDevice);
            charactersBlocked = false;

            for (var control = focused; control != null; control = control.Parent)
            {
                control.Gestures.Evaluate(gameTime, this);

                if (control.Gestures.BlockedDevices.Contains(type))
                    break;
            }

            ui.EvaluateGlobalGestures(gameTime, this, blocked);

            blocked.Clear();
        }

        public void BlockInputs(IEnumerable<int> inputs)
        {
            blocked.AddRange(inputs);
            if (!charactersBlocked && inputs.Contains(-1))
                charactersBlocked = true;
        }

        public bool IsBlocked(ICollection<int> inputs)
        {
            foreach (var item in inputs)
            {
                if (charactersBlocked && (item == -1 || ((Keys)item).IsCharacterKey()))
                    return true;

                if (blocked.Contains(item))
                    return true;
            }

            return false;
        }

        public bool IsKeyDown(Keys key)
        {
            return currentState.IsKeyDown(key);
        }

        public bool IsKeyUp(Keys key)
        {
            return currentState.IsKeyUp(key);
        }

        public bool WasKeyDown(Keys key)
        {
            return previousState.IsKeyDown(key);
        }

        public bool WasKeyUp(Keys key)
        {
            return previousState.IsKeyUp(key);
        }

        public bool IsKeyNewlyDown(Keys key)
        {
            return IsKeyDown(key) && WasKeyUp(key);
        }

        public bool IsKeyNewlyUp(Keys key)
        {
            return IsKeyUp(key) && WasKeyDown(key);
        }
    }
}
