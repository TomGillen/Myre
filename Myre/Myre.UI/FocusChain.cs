using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Collections;

namespace Myre.UI
{
    public class FocusChain
    {
        //private static Pool<WeakReference> weakReferencePool = new Pool<WeakReference>();
        private static Predicate<FocusRecord> disposedRecords = delegate(FocusRecord record) { var c = record.Control; return c == null || c.IsDisposed; };
        private static Func<Control, int> byFocusPriority = control => control.FocusPriority;

        private int idCounter;
        private List<FocusRecord> previous;
        private Control focused;
        private Control root;
        private List<Control> unfocused;
        private List<Control> newFocused;

        public Control FocusedControl
        {
            get { return focused; }
        }

        public Control FocusRoot
        {
            get { return root; }
        }

        public FocusChain()
        {
            previous = new List<FocusRecord>();
            unfocused = new List<Control>();
            newFocused = new List<Control>();
        }

        public FocusRecord? Focus(Control control)
        {
            Focus(control, true);
            return PreviousFocus();
        }

        public void RestorePrevious(FocusRecord? hint)
        {
            FocusRecord record;
            Control control = null;
            do
            {
                if (previous.Count == 0)
                {
                    control = null;
                    break;
                }

                record = previous[previous.Count - 1];
                previous.RemoveAt(previous.Count - 1);
                control = record.Control;
            } while ((control == null || control.IsDisposed)
                  || (hint != null && hint.Value.ID < record.ID));

            Focus(control, false);
        }

        protected FocusRecord? PreviousFocus()
        {
            if (previous.Count > 0)
                return previous[previous.Count - 1];
            else
                return null;
        }

        protected virtual void AddFocus(Control control) { }

        protected virtual void RemoveFocus(Control control) { }

        protected virtual void Focus(Control control, bool rememberPrevious)
        {
            if (control == focused)
                return;

            if (control != null && !control.LikesHavingFocus)
            {
                foreach (var item in control.Children.OrderBy(byFocusPriority))
                {
                    if (item.LikesHavingFocus)
                    {
                        control = item;
                        break;
                    }
                }
            }

            // find all old controls being unfocused
            for (var c = focused; c != null; c = c.Parent)
                unfocused.Add(c);

            // find all new controls being focused
            for (var c = control; c != null; c = c.Parent)
                newFocused.Add(c);

            var newRoot = newFocused.Count > 0 ? newFocused[newFocused.Count - 1] : null;

            // remove all the common controls
            // walk both lists backwards (from the shared root) until the trees diverge
            for (int i = newFocused.Count - 1; i >= 0; i--)
            {
                if (unfocused.Count > 0 && newFocused[i] == unfocused[unfocused.Count - 1])
                {
                    newFocused.RemoveAt(i);
                    unfocused.RemoveAt(unfocused.Count - 1);
                }
                else
                {
                    break;
                }
            }

            // walk through the old controls, from leaf towards root, unfocusing them
            for (int i = 0; i < unfocused.Count; i++)
            {
                RemoveFocus(unfocused[i]);
                unfocused[i].FocusedCount--;
            }

            // walk through the new controls, from root to leaf, focusing them
            for (int i = newFocused.Count - 1; i >= 0; i--)
            {
                AddFocus(newFocused[i]);
                newFocused[i].FocusedCount++;
            }

            // push old control onto stack
            if (rememberPrevious && focused != null)
            {
                unchecked
                {
                    idCounter++;
                }

                var record = new FocusRecord() { ID = idCounter, Control = focused };
                previous.Add(record);
            }

            // remember new control
            focused = control;
            root = newRoot;

            // clear buffers
            unfocused.Clear();
            newFocused.Clear();

            CompactPreviousList();
        }

        private void CompactPreviousList()
        {
            for (int i = previous.Count - 1; i >= 0; i--)
            {
                if (disposedRecords(previous[i]))
                    previous.RemoveAt(i);
            }
            
            //previous.RemoveAll(disposedRecords);
        }

        public struct FocusRecord
        {
            private WeakReference reference;
            public int ID;
            public Control Control
            {
                get { return reference.Target as Control; }
                set
                {
                    reference = new WeakReference(value);
                    //reference = weakReferencePool.Get();
                    //reference.Target = value;
                }
            }
        }
    }
}
