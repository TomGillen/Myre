using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre.Graphics
{
    /// <summary>
    /// An interface defining an object which can perform its' own drawing during a RenderablePhase.
    /// </summary>
    /// <remarks>
    /// Behaviour managers which implement this interface will be drawn during any RenderablePhase added to the Renderers' plan.
    /// </remarks>
    public interface IRenderable
    {
        void Draw(Renderer renderer);
    }
}
