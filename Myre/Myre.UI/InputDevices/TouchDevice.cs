#if !XNA_3_1
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Myre.UI.InputDevices
{
    public class TouchDevice
        : IInputDevice
    {
        private TouchCollection touches;
        private List<int> blocked;
        private List<Control> buffer;
        private List<Control> current;
        private List<Control> previous;
        private IEnumerable<Control> warmed;
        private IEnumerable<Control> cooled;

        public InputActor Owner { get; set; }

        public TouchLocation Current { get; private set; }

        public void Update(GameTime gameTime)
        {
            touches = TouchPanel.GetState();
            buffer = new List<Control>();
            current = new List<Control>();
            previous = new List<Control>();
            blocked = new List<int>();

            cooled = previous.Except(current).Distinct();
            warmed = current.Except(previous).Distinct();
        }

        public void Evaluate(GameTime gameTime, Control focused, UserInterface ui)
        {
            var type = typeof(TouchDevice);

            for (int i = 0; i < touches.Count; i++)
            {
                var t = touches[i];

                ui.FindControls(t.Position, buffer);
                current.AddRange(buffer);

                for (int j = 0; j < buffer.Count; j++)
                {
                    buffer[j].Gestures.Evaluate(gameTime, this);

                    if (buffer[j].Gestures.BlockedDevices.Contains(type))
                        break;
                }

                ui.EvaluateGlobalGestures(gameTime, this, blocked);
                blocked.Clear();
                buffer.Clear();
            }

            foreach (var item in cooled)
                item.HeatCount--;
            foreach (var item in warmed)
                item.HeatCount++;

            previous.Clear();
            previous.AddRange(current);
            current.Clear();
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
    }
}
#endif