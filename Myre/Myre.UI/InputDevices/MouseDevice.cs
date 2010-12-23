using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Myre.UI.InputDevices
{
    public class MouseDevice
        : IInputDevice
    {
        private MouseState previousState;
        private MouseState currentState;
        private List<Control> controls;
        private List<Control> previous;
        private IEnumerable<Control> cooled;
        private IEnumerable<Control> warmed;
        private List<int> blocked;

        public InputActor Owner { get; set; }

        public Vector2 Position
        {
            get { return new Vector2(currentState.X, currentState.Y); }
            set { Mouse.SetPosition((int)value.X, (int)value.Y); }
        }

        public Vector2 PositionMovement
        {
            get { return new Vector2(currentState.X - previousState.X, currentState.Y - previousState.Y); }
        }

        public float Wheel
        {
            get { return currentState.ScrollWheelValue; }
        }

        public float WheelMovement
        {
            get { return currentState.ScrollWheelValue - previousState.ScrollWheelValue; }
        }

        public MouseDevice()
        {
            previousState = currentState = Mouse.GetState();
            controls = new List<Control>();
            blocked = new List<int>();
            previous = new List<Control>();

            cooled = previous.Except(controls);
            warmed = controls.Except(previous);
        }

        public void Update(GameTime gameTime)
        {
            previousState = currentState;
            currentState = Mouse.GetState();
        }

        public void Evaluate(GameTime gameTime, Control focused, UserInterface ui)
        {
            ui.FindControls(Position, controls);

            foreach (var item in cooled)
                item.HeatCount--;
            foreach (var item in warmed)
                item.HeatCount++;

            var type = typeof(MouseDevice);

            for (int i = 0; i < controls.Count; i++)
            {
                controls[i].Gestures.Evaluate(gameTime, this);

                if (controls[i].Gestures.BlockedDevices.Contains(type))
                    break;
            }

            ui.EvaluateGlobalGestures(gameTime, this, blocked);

            previous.Clear();
            previous.AddRange(controls);
            blocked.Clear();
            controls.Clear();
        }

        public void BlockInputs(IEnumerable<int> inputs)
        {
            blocked.AddRange(inputs);
        }

        public bool IsBlocked(ICollection<int> inputs)
        {
            foreach (var item in inputs)
            {
                if (blocked.Contains(item))
                    return true;
            }

            return false;
        }

        public bool IsButtonDown(MouseButtons button)
        {
            return currentState.IsButtonDown(button);
        }

        public bool IsButtonUp(MouseButtons button)
        {
            return currentState.IsButtonUp(button);
        }

        public bool WasButtonDown(MouseButtons button)
        {
            return previousState.IsButtonDown(button);
        }

        public bool WasButtonUp(MouseButtons button)
        {
            return previousState.IsButtonUp(button);
        }

        public bool IsButtonNewlyDown(MouseButtons button)
        {
            return IsButtonDown(button) && WasButtonUp(button);
        }

        public bool IsButtonNewlyUp(MouseButtons button)
        {
            return IsButtonUp(button) && WasButtonDown(button);
        }

        ~MouseDevice()
        {
            foreach (var item in previous)
            {
                if (!item.IsDisposed)
                    item.HeatCount--;
            }
        }
    }
}
