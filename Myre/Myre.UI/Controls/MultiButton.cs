﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myre.UI.Gestures;
using Myre.UI.InputDevices;

namespace Myre.UI.Controls
{
    /// <summary>
    /// An abstract button control, where the button can have multiple values swichable by the user.
    /// </summary>
    public abstract class MultiButton
        : Button
    {
        private int count;
        private int selectedOption;

        /// <summary>
        /// Gets or sets the number of options this <c>MultiButton</c> holds.
        /// </summary>
        public int OptionsCount 
        {
            get { return count; }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("value", "value cannot be < 1.");
                count = value;
                selectedOption = Math.Min(selectedOption, count - 1);
            }
        }

        /// <summary>
        /// Gets or sets the selected option index.
        /// </summary>
        /// <value>The selected option.</value>
        public int SelectedOption
        {
            get { return selectedOption; }
            set
            {
                System.Diagnostics.Debug.WriteLine(DateTime.Now);
                while (value < 0)
                    value += count;
                value %= count;
                if (selectedOption != value)
                {
                    selectedOption = value;
                    OnSelectionChanged();
                }
            }
        }

        /// <summary>
        /// An event which is raised when the selected option changes.
        /// </summary>
        public event ControlEventHandler SelectionChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiButton"/> class.
        /// </summary>
        /// <param name="parent">This controls parent control.</param>
        public MultiButton(Control parent)
            : base(parent)
        {
            BindGestures();
        }

        /// <summary>
        /// Binds the left and right buttons to next and previous.
        /// </summary>
        private void BindGestures()
        {
            Gestures.Bind(PreviousOption,
               new ButtonPressed(Buttons.DPadLeft),
               new ButtonPressed(Buttons.LeftThumbstickLeft),
               new KeyPressed(Keys.Left));

            Gestures.Bind(NextOption,
                new ButtonPressed(Buttons.DPadRight),
                new ButtonPressed(Buttons.LeftThumbstickRight),
                new KeyPressed(Keys.Right));
        }

        private void NextOption(IGesture gesture, GameTime time, IInputDevice device)
        {
            SelectedOption++;
        }

        private void PreviousOption(IGesture gesture, GameTime time, IInputDevice device)
        {
            SelectedOption--;
        }

        /// <summary>
        /// Called when the selection changes.
        /// </summary>
        protected virtual void OnSelectionChanged()
        {
            if (SelectionChanged != null)
                SelectionChanged(this);
        }
    }
}
