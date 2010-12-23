﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace Myre.Graphics.Pipeline
{
    public class MyreMaterialData
    {
        public string EffectName;

        [ContentSerializer(Optional = true)]
        public Dictionary<string, string> Textures = new Dictionary<string, string>();

        [ContentSerializer(Optional = true)]
        public Dictionary<string, object> OpaqueData = new Dictionary<string, object>();

        [ContentSerializer(Optional = true)]
        public string Technique;
    }
}
