using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Myre.Collections;
using System.Diagnostics;

namespace Myre.Graphics.Materials
{
    public class Material
    {
        private Effect effect;
        private MaterialParameter[] parameters;

        public EffectParameterCollection Parameters
        {
            get { return effect.Parameters; }
        }

        public EffectTechniqueCollection Techniques
        {
            get { return effect.Techniques; }
        }

        public EffectTechnique CurrentTechnique
        {
            get { return effect.CurrentTechnique; }
            set { effect.CurrentTechnique = value; }
        }

        public Material(Effect effect, string techniqueName = null)
        {
            this.effect = effect;
            effect.CurrentTechnique = effect.Techniques[techniqueName] ?? effect.Techniques[0];
            this.parameters = (from p in effect.Parameters
                               where !string.IsNullOrEmpty(p.Semantic) //&& technique.IsParameterUsed(p) <-- why did xna 4.0 remove this?!
                               select new MaterialParameter(p)).ToArray();
        }

        public IEnumerable<EffectPass> Begin(BoxedValueStore<string> parameterValues)
        {
            for (int i = 0; i < parameters.Length; i++)
                parameters[i].Apply(parameterValues);

            return effect.CurrentTechnique.Passes;
        }
    }
}
