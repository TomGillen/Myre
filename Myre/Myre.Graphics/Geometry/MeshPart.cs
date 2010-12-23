//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Myre.Graphics.Materials;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Content;

//namespace Myre.Graphics.Geometry
//{
//    public class MeshPart
//    {
//        public int TriangleCount { get; set; }
//        public int VertexCount { get; set; }
//        public VertexBuffer VertexBuffer { get; set; }
//        public IndexBuffer IndexBuffer { get; set; }
//        public Dictionary<string, Material> Materials { get; set; }
//    }

//    public class MeshPartReader : ContentTypeReader<MeshPart>
//    {
//        protected override MeshPart Read(ContentReader input, MeshPart existingInstance)
//        {
//            var part = existingInstance ?? new MeshPart();

//            part.VertexCount = input.ReadInt32();
//            part.TriangleCount = input.ReadInt32();
//            part.VertexBuffer = input.ReadObject<VertexBuffer>();
//            part.IndexBuffer = input.ReadObject<IndexBuffer>();

//            var size = input.ReadInt32();
//            part.Materials = new Dictionary<string, Materials.Material>(size);
//            for (int i = 0; i < size; i++)
//            {
//                var key = input.ReadObject<string>();
//                var value = input.ReadObject<Material>();
//                part.Materials.Add(key, value);
//            }

//            return part;
//        }
//    }
//}
