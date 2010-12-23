using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre.UI
{
    public class InputActorCollection
        : List<InputActor>
    {
        public void AllFocus(Control control)
        {
            foreach (var actor in this)
                actor.Focus(control);
        }
    }
}
