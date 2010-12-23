using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Myre.Graphics.Geometry
{
    public class ModelData
    {
        public Mesh[] Meshes { get; set; }
        //public ModelBone[] Skeleton { get; set; }
    }

    public class ModelReader : ContentTypeReader<ModelData>
    {
        protected override ModelData Read(ContentReader input, ModelData existingInstance)
        {
            var model = existingInstance ?? new ModelData();

            var size = input.ReadInt32();
            model.Meshes = new Mesh[size];
            for (int i = 0; i < size; i++)
                model.Meshes[i] = input.ReadObject<Mesh>();

            //size = input.ReadInt32();
            //model.Skeleton = new ModelBone[size];
            //for (int i = 0; i < size; i++)
            //    model.Skeleton[i] = input.ReadObject<ModelBone>();

            return model;
        }
    }
}