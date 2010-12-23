using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Myre.UI.InputDevices;

namespace Myre.UI
{
    public class InputActor
        : FocusChain, IGameComponent, IUpdateable, ICollection<IInputDevice>
    {
        private IList<IInputDevice> devices;
        private int id;
        private bool enabled;
        private int updateOrder;

        public IInputDevice this[int i]
        {
            get { return devices[i]; }
            set
            { 
                devices[i] = value;
                devices[i].Owner = this;
            }
        }

        public int ID
        {
            get { return id; }
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

        #endregion

        public InputActor(int id)
        {
            this.id = id;
            this.devices = new List<IInputDevice>();
            this.enabled = true;
        }

        public InputActor(int id, params IInputDevice[] inputs)
            :this(id)
        {
            for (int i = 0; i < inputs.Length; i++)
                Add(inputs[i]);
        }

        public void Initialize()
        {
        }

        public void Update(GameTime gameTIme)
        {
            if (FocusedControl != null && FocusedControl.IsDisposed)
                RestorePrevious(null);

            foreach (var device in devices)
                device.Update(gameTIme);
        }

        public T FindDevice<T>()
            where T : class, IInputDevice
        {
            foreach (var item in this)
            {
                if (item is T)
                    return item as T;
            }

            return null;
        }

        protected override void Focus(Control control, bool rememberPrevious)
        {
            if (control != null && !control.UserInterface.Actors.Contains(this))
                throw new InvalidOperationException("This actor does not belong to the specified UserInterface.");

            base.Focus(control, rememberPrevious);
        }

        protected override void AddFocus(Control control)
        {
            control.focusedBy.Add(new ActorFocus() { Actor = this, Record = PreviousFocus() });
            base.AddFocus(control);
        }

        protected override void RemoveFocus(Control control)
        {
            for (int i = control.focusedBy.Count - 1; i >= 0 ; i--)
            {
                if (control.focusedBy[i].Actor == this)
                    control.focusedBy.RemoveAt(i);
            }

            base.RemoveFocus(control);
        }

        internal void Evaluate(GameTime gameTime, UserInterface ui)
        {
            foreach (var device in devices)
                device.Evaluate(gameTime, FocusRoot == ui.Root ? FocusedControl : null, ui);
        }

        #region ICollection<IInputDevice> Members

        public void Add(IInputDevice item)
        {
            devices.Add(item);
            item.Owner = this;
        }

        public void Clear()
        {
            devices.Clear();
        }

        public bool Contains(IInputDevice item)
        {
            return devices.Contains(item);
        }

        public void CopyTo(IInputDevice[] array, int arrayIndex)
        {
            devices.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return devices.Count; }
        }

        public bool IsReadOnly
        {
            get { return devices.IsReadOnly; }
        }

        public bool Remove(IInputDevice item)
        {
            if (devices.Remove(item))
            {
                item.Owner = null;
                return true;
            }
            else
            {
                return false;
            }            
        }

        #endregion

        #region IEnumerable<IInputDevice> Members

        public IEnumerator<IInputDevice> GetEnumerator()
        {
            return devices.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}