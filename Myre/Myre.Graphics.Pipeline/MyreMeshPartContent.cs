//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Microsoft.Xna.Framework.Content.Pipeline.Processors;
//using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
//using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
//using Microsoft.Xna.Framework.Content.Pipeline;

//namespace Myre.Graphics.Pipeline
//{
//    public class MyreMeshPartContent
//    {
//        public int TriangleCount { get; set; }

//        public int VertexCount { get; set; }

//        public VertexBufferContent VertexBuffer { get; set; }

//        public IndexCollection IndexBuffer { get; set; }

//        [ContentSerializer(SharedResource = true)]
//        public Dictionary<string, MyreMaterialContent> Materials { get; set; }
//    }

//    /// <summary>
//    /// This class will be instantiated by the XNA Framework Content Pipeline
//    /// to write the specified data type into binary .xnb format.
//    ///
//    /// This should be part of a Content Pipeline Extension Library project.
//    /// </summary>
//    [ContentTypeWriter]
//    public class MeshPartContentWriter : ContentTypeWriter<MyreMeshPartContent>
//    {
//        protected override void Write(ContentWriter output, MyreMeshPartContent value)
//        {
//            output.Write(value.VertexCount);
//            output.Write(value.TriangleCount);
//            output.WriteObject(value.VertexBuffer);
//            output.WriteObject(value.IndexBuffer);

//            // manually write out the dictionary, as the dictionary reader class DOES NOT EXIST
//            output.Write(value.Materials.Count);
//            foreach (var item in value.Materials)
//            {
//                output.WriteObject(item.Key);
//                output.WriteObject(item.Value);
//            }
//        }

//        public override string GetRuntimeReader(TargetPlatform targetPlatform)
//        {
//            return "Myre.Graphics.Geometry.MeshPartReader, Myre.Graphics";
//        }
//    }
//}
