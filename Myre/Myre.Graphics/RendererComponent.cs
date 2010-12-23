using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Collections;
using System.Collections.ObjectModel;
using Myre.Collections;
using Microsoft.Xna.Framework.Graphics;

namespace Myre.Graphics
{
    public abstract class RendererComponent
    {
        public class Resource
        {
            public string Name;
            public Action<Renderer, Resource> Finaliser;
            public bool IsLeftSet;

            public Resource()
            {
                Name = null;
                Finaliser = (r, o) => r.FreeResource(o.Name);
            }

            internal void Finalise(Renderer renderer)
            {
                Finaliser(renderer, this);
            }
        }

        public class Input
        {
            public string Name;
            public bool Optional;
        }

        public ReadOnlyCollection<Input> InputResources { get; private set; }
        public ReadOnlyCollection<Resource> OutputResources { get; private set; }
        public RenderTargetInfo? OutputTarget { get; private set; }
        public bool Initialised { get; private set; }
        
        protected abstract void SpecifyResources(IList<Input> inputResources, IList<Resource> outputResources, out RenderTargetInfo? outputTarget);
        protected internal abstract bool ValidateInput(RenderTargetInfo? previousRenderTarget);

        internal void SpecifyResources()
        {
            var inputs = new List<Input>();
            var outputs = new List<Resource>();
            RenderTargetInfo? outputTarget;
            SpecifyResources(inputs, outputs, out outputTarget);

            this.InputResources = new ReadOnlyCollection<Input>(inputs);
            this.OutputResources = new ReadOnlyCollection<Resource>(outputs);
            this.OutputTarget = outputTarget;
        }

        public abstract RenderTarget2D Draw(Renderer renderer);
        public virtual void Initialise(Renderer renderer) { Initialised = true; }
    }
}
