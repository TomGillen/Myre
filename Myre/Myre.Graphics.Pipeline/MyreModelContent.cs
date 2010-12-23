using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace Myre.Graphics.Pipeline
{
    [ContentSerializerRuntimeType("Myre.Graphics.Geometry.ModelData, Myre.Graphics")]
    public class MyreModelContent
    {
        private List<MyreMeshContent> meshes;

        public MyreMeshContent[] Meshes { get { return meshes.ToArray(); } }
        //public BoneContent[] Skeleton { get; set; }

        public MyreModelContent()
        {
            meshes = new List<MyreMeshContent>();
        }

        internal void AddMesh(MyreMeshContent mesh)
        {
            meshes.Add(mesh);
        }
    }

    [ContentTypeWriter]
    public class MyreModelContentWriter : ContentTypeWriter<MyreModelContent>
    {
        protected override void Write(ContentWriter output, MyreModelContent value)
        {
            output.Write(value.Meshes.Length);
            foreach (var item in value.Meshes)
            {
                output.WriteObject(item);
            }

            //output.Write(value.Skeleton.Length);
            //foreach (var item in value.Skeleton)
            //{
            //    output.WriteObject(item);
            //}
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Geometry.ModelReader, Myre.Graphics";
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "Myre.Graphics.Geometry.ModelData, Myre.Graphics";
        }
    }
}
