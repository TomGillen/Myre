using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Myre.UI.InputDevices
{
    /// <summary>
    /// The left or right.
    /// </summary>
    public enum Side
    {
        /// <summary>
        /// The left.
        /// </summary>
        Left,

        /// <summary>
        /// The right.
        /// </summary>
        Right
    }

    public class GamepadDevice
        : IInputDevice
    {
        public PlayerIndex Player { get; private set; }
        private GamePadState currentState;
        private GamePadState previousState;
        private GamePadThumbSticks currentThumbsticks;
        private GamePadThumbSticks previousThumbsticks;
        private float minDeadZone = 0.25f;
        private List<int> blocked;

        public InputActor Owner { get; set; }

        public Vector2 LeftThumbstick
        {
            get { return currentState.ThumbSticks.Left; }
        }

        public Vector2 LeftThumbstickMovement
        {
            get { return currentState.ThumbSticks.Left - previousState.ThumbSticks.Left; }
        }

        public Vector2 RightThumbstick
        {
            get { return currentState.ThumbSticks.Right; }
        }

        public Vector2 RightThumbstickMovement
        {
            get { return currentState.ThumbSticks.Right - previousState.ThumbSticks.Right; }
        }

        public float LeftTrigger
        {
            get { return currentState.Triggers.Left; }
        }

        public float LeftTriggerMovement
        {
            get { return currentState.Triggers.Left - previousState.Triggers.Left; }
        }

        public float RightTrigger
        {
            get { return currentState.Triggers.Right; }
        }

        public float RightTriggerMovement
        {
            get { return currentState.Triggers.Right - previousState.Triggers.Right; }
        }

        public GamepadDevice(PlayerIndex player)
        {
            this.Player = player;
            previousState = currentState = GamePad.GetState(player);
            blocked = new List<int>();
        }

        public void Update(GameTime gameTime)
        {
            previousState = currentState;
            currentState = GamePad.GetState(Player, GamePadDeadZone.IndependentAxes);

            previousThumbsticks = currentThumbsticks;
            currentThumbsticks = GamePad.GetState(Player, GamePadDeadZone.None).ThumbSticks;
        }

        public void Evaluate(GameTime gameTime, Control focused, UserInterface ui)
        {
            if (!currentState.IsConnected)
                return;

            var type = typeof(GamepadDevice);
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

        public Vector2 ApplyDeadZone(Vector2 direction, GamePadDeadZone deadZone, float power)
        {
            switch (deadZone)
            {
                case GamePadDeadZone.Circular:
                    return RescaleMagnitude(direction, power);
                case GamePadDeadZone.IndependentAxes:
                    float x = RescaleAxis(direction.X, power);
                    float y = RescaleAxis(direction.Y, power);
                    return new Vector2(x, y);
                default:
                    return direction;
            }
        }

        private Vector2 RescaleMagnitude(Vector2 direction, float power)
        {
            float magnitude = direction.Length();

            if (magnitude == 0)
                return Vector2.Zero;

            float targetMagnitude = Rescale(magnitude, minDeadZone, 1f);
            targetMagnitude = (float)Math.Pow(targetMagnitude, power);
            return direction * (targetMagnitude / magnitude);
        }

        private float RescaleAxis(float value, float power)
        {
            if (value > 0)
                value = Rescale(value, minDeadZone, 1f);
            else
                value = -Rescale(-value, minDeadZone, 1f);

            return (float)Math.Pow(value, power);
        }

        private static float Rescale(float value, float min, float max)
        {
            var range = max - min;
            var alpha = (value - min) / max;
            return MathHelper.Clamp(alpha * range, min, max);
        }

        public bool IsButtonDown(Buttons button)
        {
            return currentState.IsButtonDown(button);
        }

        public bool IsButtonUp(Buttons button)
        {
            return currentState.IsButtonUp(button);
        }

        public bool WasButtonDown(Buttons button)
        {
            return previousState.IsButtonDown(button);
        }

        public bool WasButtonUp(Buttons button)
        {
            return previousState.IsButtonUp(button);
        }

        public bool IsButtonNewlyDown(Buttons button)
        {
            return IsButtonDown(button) && WasButtonUp(button);
        }

        public bool IsButtonNewlyUp(Buttons button)
        {
            return IsButtonUp(button) && WasButtonDown(button);
        }
    }
}
