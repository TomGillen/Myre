using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Myre
{
    /// <summary>
    /// A static class containing methods for fire-and-forget delayed events and transitions.
    /// </summary>
    public static class Delayed
    {
        static List<Event> events = new List<Event>();
        static List<Event> buffer = new List<Event>();

        /// <summary>
        /// Fires the spevified action after the specified number of seconds have elapsed.
        /// </summary>
        /// <param name="action">The delegate to execute.</param>
        /// <param name="delay">The delay before the action is executed.</param>
        public static void Action(Action action, float delay)
        {
            buffer.Add(new Event()
            {
                Start = DateTime.Now,
                Duration = delay,
                Completed = action
            });
        }

        /// <summary>
        /// Calls the specified delegate every frame for the specified number of seconds.
        /// </summary>
        /// <param name="step">The method to call each frame. This method takes on float parameter which is the progress from 0 to 1.</param>
        /// <param name="duration">The number of seconds to call the delegate for.</param>
        public static void Transition(Action<float> step, float duration)
        {
            Transition(step, duration, null);
        }

        /// <summary>
        /// Calls the specified delegate every frame for the specified number of seconds, and then calls the specified callback delegate.
        /// </summary>
        /// <param name="step">The method to call each frame. This method takes on float parameter which is the progress from 0 to 1.</param>
        /// <param name="completionCallback">The method to call on completion of the transition.</param>
        /// <param name="duration">The number of seconds to call the delegate for.</param>
        public static void Transition(Action<float> step, float duration, Action completionCallback)
        {
            buffer.Add(new Event()
            {
                Start = DateTime.Now,
                Duration = duration,
                Completed = completionCallback,
                Transition = step
            });
        }

        /// <summary>
        /// Updates all transitions. This is called by MyreGame.Update(gameTime).
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Update(GameTime gameTime)
        {
            var now = DateTime.Now;

            events.AddRange(buffer);
            buffer.Clear();

            for (int i = events.Count - 1; i >= 0; i--)
            {
                var e = events[i];
                e.Progress = MathHelper.Clamp((float)(now - e.Start).TotalSeconds / e.Duration, 0, 1);

                if (e.Transition != null)
                    e.Transition(e.Progress);

                if (e.Progress == 1)
                {
                    if (e.Completed != null)
                        e.Completed();

                    events.RemoveAt(i);
                }
                else
                    events[i] = e;
            }
        }
    }

    struct Event
    {
        public DateTime Start;
        public float Duration;
        public float Progress;
        public Action Completed;
        public Action<float> Transition;
    }
}
