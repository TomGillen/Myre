using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Myre.Extensions;
using Microsoft.Xna.Framework.Graphics;
using Myre.UI.Gestures;
using Myre.UI.InputDevices;
using Microsoft.Xna.Framework.GamerServices;

namespace Myre.UI
{
    public class UserInterface
        : IGameComponent, IDrawable, IUpdateable
    {
        private List<Control> buffer;
        private SpriteBatch spriteBatch;
        private InputActorCollection actors;
        private int drawOrder;
        private bool visible;
        private int updateOrder;
        private bool enabled;
        private bool enableInput;
        private Dictionary<Type, List<IGesturePair>> globalGestures;

        public Control Root
        {
            get;
            private set;
        }

        internal Dictionary<Type, List<IGesturePair>> GlobalGestures
        {
            get { return globalGestures; }
        }

        public InputActorCollection Actors
        {
            get { return actors; }
        }

        public bool EnableInput
        {
            get { return enableInput; }
            set { enableInput = value; }
        }

        public GraphicsDevice Device
        {
            get;
            private set;
        }

        #region IUpdateable Members

        public bool Enabled
        {
            get { return enabled; }
            set
            {
                if (enabled != value)
                {
                    enabled = value;
                    OnEnabledChanged();
                }
            }
        }

        public int UpdateOrder
        {
            get { return updateOrder; }
            set
            {
                if (updateOrder != value)
                {
                    updateOrder = value;
                    OnUpdateOrderChanged();
                }
            }
        }

#if XNA_3_1
        public event EventHandler UpdateOrderChanged;
        public event EventHandler EnabledChanged;
#else
        public event EventHandler<EventArgs> UpdateOrderChanged;
        public event EventHandler<EventArgs> EnabledChanged;
#endif

        #endregion

        #region IDrawable Members
        public int DrawOrder
        {
            get { return drawOrder; }
            set
            {
                if (drawOrder != value)
                {
                    drawOrder = value;
                    OnDrawOrderChanged();
                }
            }
        }

        public bool Visible
        {
            get { return visible; }
            set
            {
                if (visible != value)
                {
                    visible = value;
                    OnVisiblChanged();
                }
            }
        }

#if XNA_3_1
        public event EventHandler DrawOrderChanged;
        public event EventHandler VisibleChanged;
#else
        public event EventHandler<EventArgs> DrawOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;
#endif
        #endregion

        public UserInterface(GraphicsDevice graphics)
        {
            Device = graphics;
            spriteBatch = new SpriteBatch(Device);
            buffer = new List<Control>();
            actors = new InputActorCollection();
            globalGestures = new Dictionary<Type, List<IGesturePair>>();
            enableInput = true;
            visible = true;
            drawOrder = 100;
            enabled = true;
            updateOrder = 0;

            Root = new Control(this);
            Root.SetPoint(Points.TopLeft, 0, 0);
            Root.SetPoint(Points.BottomRight, 0, 0);
        }

        public void Initialize()
        {
        }

        public void Update(GameTime gameTime)
        {
            if (EnableInput && Visible
#if WINDOWS
                )
#else
                && !Guide.IsVisible)
#endif
            {
                foreach (var actor in actors)
                    actor.Evaluate(gameTime, this);
            }

            buffer.Clear();
            AddControlsToBuffer(Root);
            buffer.InsertionSort(ControlStrataComparer.BottomToTop);

            for (int i = 0; i < buffer.Count; i++)
                buffer[i].Update(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
#if XNA_3_1
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
#else
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
#endif
            for (int i = 0; i < buffer.Count; i++)
            {
                if (buffer[i].IsVisible)
                    buffer[i].Draw(spriteBatch);
            }
            spriteBatch.End();
        }

        public void EvaluateGlobalGestures(GameTime gameTime, IInputDevice device, ICollection<int> blocked)
        {
            var deviceType = device.GetType();
            if (globalGestures.ContainsKey(deviceType))
            {
                var gestures = globalGestures[deviceType];
                foreach (var gesture in gestures)
                {
                    if (!gesture.Evaluated)
                        gesture.Evaluate(gameTime, device);

                    gesture.Evaluated = false;
                }
            }
        }

        public void FindControls(Vector2 point, ICollection<Control> results)
        {
            for (int i = buffer.Count - 1; i >= 0; i--)
            {
                var control = buffer[i];
                if (control.IsVisible && control.Contains(point))
                    results.Add(control);
            }
        }

        protected virtual void OnDrawOrderChanged()
        {
            if (DrawOrderChanged != null)
                DrawOrderChanged(this, new EventArgs());
        }

        protected virtual void OnVisiblChanged()
        {
            if (VisibleChanged != null)
                VisibleChanged(this, new EventArgs());
        }

        protected virtual void OnEnabledChanged()
        {
            if (EnabledChanged != null)
                EnabledChanged(this, new EventArgs());
        }

        protected virtual void OnUpdateOrderChanged()
        {
            if (UpdateOrderChanged != null)
                UpdateOrderChanged(this, new EventArgs());
        }

        private void AddControlsToBuffer(Control control)
        {
            buffer.Add(control);
            foreach (var child in control.Children)
                AddControlsToBuffer(child);
        }
    }
}
