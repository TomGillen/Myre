using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.IO;
using System.Diagnostics;

namespace Myre.Graphics.Pipeline
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    ///
    /// TODO: change the ContentProcessor attribute to specify the correct
    /// display name for this processor.
    /// </summary>
    [ContentProcessor(DisplayName = "Myre.Graphics.Pipeline.MyreModelProcessor")]
    public class MyreModelProcessor : ContentProcessor<NodeContent, MyreModelContent>
    {

        ContentProcessorContext context;
        MyreModelContent outputModel;
        string directory;

        // A single material may be reused on more than one piece of geometry.
        // This dictionary keeps track of materials we have already converted,
        // to make sure we only bother processing each of them once.
        Dictionary<MaterialContent, Dictionary<string, MyreMaterialContent>> processedMaterials =
                            new Dictionary<MaterialContent, Dictionary<string, MyreMaterialContent>>();

        public string DiffuseTexture
        {
            get;
            set;
        }

        public string SpecularTexture
        {
            get;
            set;
        }

        public string NormalTexture
        {
            get;
            set;
        }


        /// <summary>
        /// Converts incoming graphics data into our custom model format.
        /// </summary>
        public override MyreModelContent Process(NodeContent input,
                                                   ContentProcessorContext context)
        {
            this.context = context;

            directory = Path.GetDirectoryName(input.Identity.SourceFilename);

            // Find the skeleton.
            BoneContent skeleton = MeshHelper.FindSkeleton(input);

            // We don't want to have to worry about different parts of the model being
            // in different local coordinate systems, so let's just bake everything.
            FlattenTransforms(input, skeleton);

            outputModel = new MyreModelContent();

            //if (skeleton != null)
            //    outputModel.Skeleton = MeshHelper.FlattenSkeleton(skeleton).ToArray();
            //else
            //    outputModel.Skeleton = new BoneContent[0];

            ProcessNode(input);

            return outputModel;
        }

        /// <summary>
        /// Bakes unwanted transforms into the model geometry,
        /// so everything ends up in the same coordinate system.
        /// </summary>
        static void FlattenTransforms(NodeContent node, BoneContent skeleton)
        {
            foreach (NodeContent child in node.Children)
            {
                // Don't process the skeleton, because that is special.
                if (child == skeleton)
                    continue;

                // Bake the local transform into the actual geometry.
                MeshHelper.TransformScene(child, child.Transform);

                // Having baked it, we can now set the local
                // coordinate system back to identity.
                child.Transform = Matrix.Identity;

                // Recurse.
                FlattenTransforms(child, skeleton);
            }
        }

        /// <summary>
        /// Recursively processes a node from the input data tree.
        /// </summary>
        private void ProcessNode(NodeContent node)
        {
            // Is this node in fact a mesh?
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                MeshHelper.OptimizeForCache(mesh);

                // calculate tangent frames for normal mapping
                var hasTangents = GeometryContainsChannel(mesh.Geometry, VertexChannelNames.Tangent(0));
                var hasBinormals = GeometryContainsChannel(mesh.Geometry, VertexChannelNames.Binormal(0));
                if (!hasTangents || !hasBinormals)
                {
                    var tangentName = hasTangents ? null : VertexChannelNames.Tangent(0);
                    var binormalName = hasBinormals ? null : VertexChannelNames.Binormal(0);
                    MeshHelper.CalculateTangentFrames(mesh, VertexChannelNames.TextureCoordinate(0), tangentName, binormalName);
                }

                //var outputMesh = new MyreMeshContent();
                //outputMesh.Parent = mesh.Parent;
                //outputMesh.BoundingSphere = BoundingSphere.CreateFromPoints(mesh.Positions);

                // Process all the geometry in the mesh.
                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    ProcessGeometry(geometry, outputModel);
                }

                //outputModel.AddMesh(outputMesh);
            }

            // Recurse over any child nodes.
            foreach (NodeContent child in node.Children)
            {
                ProcessNode(child);
            }
        }

        private bool GeometryContainsChannel(GeometryContentCollection geometry, string channel)
        {
            foreach (var item in geometry)
            {
                if (item.Vertices.Channels.Contains(channel))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Converts a single piece of input geometry into our custom format.
        /// </summary>
        void ProcessGeometry(GeometryContent geometry, MyreModelContent model)
        {
            int triangleCount = geometry.Indices.Count / 3;
            int vertexCount = geometry.Vertices.VertexCount;

            // Flatten the flexible input vertex channel data into
            // a simple GPU style vertex buffer byte array.
            var vertexBufferContent = geometry.Vertices.CreateVertexBuffer();

            // Convert the input material.
            var materials = ProcessMaterial(geometry.Material, geometry.Parent);

            var boundingSphere = BoundingSphere.CreateFromPoints(geometry.Vertices.Positions);
            
            // Add the new piece of geometry to our output model.
            model.AddMesh(new MyreMeshContent()
            {
                //Parent = geometry.Parent,
                Name = geometry.Parent.Name,
                BoundingSphere = boundingSphere,
                Materials = materials,
                IndexBuffer = geometry.Indices,
                VertexBuffer = vertexBufferContent,
                VertexCount = vertexCount,
                TriangleCount = triangleCount
            });
        }


        /// <summary>
        /// Creates default materials suitable for rendering in the myre deferred renderer.
        /// The current material is searched for diffuse, normal and specular textures.
        /// </summary>
        Dictionary<string, MyreMaterialContent> ProcessMaterial(MaterialContent material, MeshContent mesh)
        {
            //material = context.Convert<MaterialContent, MaterialContent>(material, "MaterialProcessor");

            // Have we already processed this material?
            if (!processedMaterials.ContainsKey(material))
            {
                // If not, process it now.
                processedMaterials[material] = new Dictionary<string, MyreMaterialContent>();
                CreateGBufferMaterial(material, mesh);
                CreateShadowMaterial(material);
            }

            return processedMaterials[material];
        }

        private void CreateShadowMaterial(MaterialContent material)
        {
            var materialData = new MyreMaterialData();
            materialData.EffectName = Path.GetFullPath("DefaultShadows.fx");
            materialData.Technique = "Technique1";

            var shadowMaterial = context.Convert<MyreMaterialData, MyreMaterialContent>(materialData, "MyreMaterialProcessor");
            processedMaterials[material].Add("shadows", shadowMaterial);
        }

        private void CreateGBufferMaterial(MaterialContent material, MeshContent mesh)
        {
            //System.Diagnostics.Debugger.Launch();
            var diffuseTexture = FindDiffuseTexture(mesh, material);
            var normalTexture = FindNormalTexture(mesh, material);
            var specularTexture = FindSpecularTexture(mesh, material);

            if (diffuseTexture == null)
                return;

            var materialData = new MyreMaterialData();
            materialData.EffectName = Path.GetFullPath("DefaultGBuffer.fx");
            materialData.Technique = "Technique1";
            materialData.Textures.Add("DiffuseMap", diffuseTexture);
            materialData.Textures.Add("NormalMap", normalTexture);
            materialData.Textures.Add("SpecularMap", specularTexture);

            var gbufferMaterial = context.Convert<MyreMaterialData, MyreMaterialContent>(materialData, "MyreMaterialProcessor");
            processedMaterials[material].Add("gbuffer", gbufferMaterial);
        }

        private string FindDiffuseTexture(MeshContent mesh, MaterialContent material)
        {
            if (string.IsNullOrEmpty(DiffuseTexture))
            {
                var texture = FindTexture(mesh, material, "texture", "diffuse", "diff", "d", "c");

                if (texture != null)
                    return texture;

                // we cant find a texture, and there isnt really any sane default to use.
                return null;
            }
            else
                return Path.Combine(directory, DiffuseTexture);
        }

        private string FindNormalTexture(MeshContent mesh, MaterialContent material)
        {
            if (string.IsNullOrEmpty(NormalTexture))
                return FindTexture(mesh, material, "normalmap", "normal", "norm", "n", "bumpmap", "bump", "b") ?? "null_normal.tga";
            else
                return Path.Combine(directory, NormalTexture);
        }

        private string FindSpecularTexture(MeshContent mesh, MaterialContent material)
        {
            if (string.IsNullOrEmpty(SpecularTexture))
                return FindTexture(mesh, material, "specularmap", "specular", "spec", "s") ?? "null_specular.tga";
            else
                return Path.Combine(directory, SpecularTexture);
        }

        private string FindTexture(MeshContent mesh, MaterialContent material, params string[] possibleKeys)
        {
            foreach (var key in possibleKeys)
            {
                // search in existing material textures
                foreach (var item in material.Textures)
                {
                    if (item.Key.ToLowerInvariant() == key)
                        return item.Value.Filename;
                }

                // search in material opaque data
                foreach (var item in material.OpaqueData)
                {
                    if (item.Key.ToLowerInvariant() == key && item.Value.GetType() == typeof(string))
                    {
                        var file = item.Value as string;
                        if (!Path.IsPathRooted(file))
                            file = Path.Combine(directory, file);

                        return file;
                    }
                }

                // search in mesh opaque data
                foreach (var item in mesh.OpaqueData)
                {
                    if (item.Key.ToLowerInvariant() == key && item.Value.GetType() == typeof(string))
                    {
                        var file = item.Value as string;
                        if (!Path.IsPathRooted(file))
                            file = Path.Combine(directory, file);

                        return file;
                    }
                }
            }

            // try and find the file in the meshs' directory
            foreach (var key in possibleKeys)
            {
                foreach (var file in Directory.EnumerateFiles(directory, mesh.Name + "_" + key + ".*", SearchOption.AllDirectories))
                    return file;
            }

            // cant find anything
            return null;
        }
    }
}