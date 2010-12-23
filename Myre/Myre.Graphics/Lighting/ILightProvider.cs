using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre.Graphics.Lighting
{
    public interface ILightProvider
    {
        bool ModifiesStencil { get; }
        bool PrepareDraw(Renderer renderer);
        void Draw(Renderer renderer);
        void DrawDebug(Renderer renderer);
    }
}
