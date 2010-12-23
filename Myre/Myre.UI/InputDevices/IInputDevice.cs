using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Myre.UI.InputDevices
{
    public interface IInputDevice
    {
        InputActor Owner { get; set; }
        void Update(GameTime gameTime);
        void Evaluate(GameTime gameTime, Control focused, UserInterface ui);
        bool IsBlocked(ICollection<int> inputs);
        void BlockInputs(IEnumerable<int> inputs);
    }
}