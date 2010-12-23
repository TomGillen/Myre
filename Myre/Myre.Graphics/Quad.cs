using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Myre.Graphics.Materials;
using Myre.Collections;

namespace Myre.Graphics
{
    public class Quad
    {
        private GraphicsDevice device;
        private Vertex[] vertices;
        private short[] indices;
        private Vector2 topLeft;
        private Vector2 bottomRight;
        private float depth;

        struct Vertex
            : IVertexType
        {
            public Vector3 Position;
            public Vector2 TexCoord;
            public float CornerIndex;

            public Vertex(Vector3 position, Vector2 texCoord, float cornerIndex)
            {
                Position = position;
                TexCoord = texCoord;
                CornerIndex = cornerIndex;
            }

            readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(20, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1)
            );

            VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
        }

        /// <summary>
        /// Gets the top left corner, in normalised device coordinates.
        /// </summary>
        /// <value>The top left.</value>
        public Vector2 TopLeft
        {
            get { return topLeft; }
        }

        /// <summary>
        /// Gets the bottom right corner, in normalised device coordinates.
        /// </summary>
        /// <value>The bottom right.</value>
        public Vector2 BottomRight
        {
            get { return bottomRight; }
        }

        /// <summary>
        /// Gets the depth.
        /// </summary>
        /// <value>The depth.</value>
        public float Depth
        {
            get { return depth; }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Quad"/> class.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        public Quad(GraphicsDevice device)
        {
            this.device = device;
            this.vertices = new Vertex[4];
            this.indices = new short[] { 0, 1, 3, 3, 1, 2 };

            SetPosition(new Vector2(-1, 1), new Vector2(1, -1), 0);
        }

        public void SetPosition(Rectangle screenCoordinates, float depth = 0)
        {
            var pp = device.PresentationParameters;
            var resolution = new Vector2(pp.BackBufferWidth, pp.BackBufferHeight);
            SetPosition(ToDeviceCoordinate(screenCoordinates.Left, screenCoordinates.Top, resolution),
                        ToDeviceCoordinate(screenCoordinates.Right, screenCoordinates.Bottom, resolution),
                        depth);

        }

        private Vector2 ToDeviceCoordinate(float x, float y, Vector2 resolution)
        {
            return new Vector2((x / resolution.X) * 2 - 1, -((y / resolution.Y) * 2 - 1));
        }

        public void SetPosition(float topLeftX = -1, float topLeftY = 1, float bottomRightX = 1, float bottomRightY = -1, float depth = 0)
        {
            SetPosition(new Vector2(topLeftX, topLeftY),
                        new Vector2(bottomRightX, bottomRightY),
                        depth);
        }

        /// <summary>
        /// Sets the poisition of this quad.
        /// </summary>
        /// <param name="topLeft">The top left corner, in normalised device coordinates.</param>
        /// <param name="bottomRight">The bottom right corner, in normalised device coordinates.</param>
        /// <param name="depth">The depth.</param>
        public void SetPosition(Vector2 topLeft, Vector2 bottomRight, float depth = 0)
        {
            this.topLeft = topLeft;
            this.bottomRight = bottomRight;
            this.depth = depth;

            vertices[0] = new Vertex(
                new Vector3(topLeft.X, topLeft.Y, depth),
                new Vector2(0, 0),
                0);
            vertices[1] = new Vertex(
                new Vector3(bottomRight.X, topLeft.Y, depth),
                new Vector2(1, 0),
                1);
            vertices[2] = new Vertex(
                new Vector3(bottomRight.X, bottomRight.Y, depth),
                new Vector2(1, 1),
                2);
            vertices[3] = new Vertex(
                new Vector3(topLeft.X, bottomRight.Y, depth),
                new Vector2(0, 1),
                3);
        }

        /// <summary>
        /// Draws this instance.
        /// </summary>
        public void Draw()
        {
            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 4, indices, 0, 2);
        }

        public void Draw(Effect effect)
        {
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Draw();
            }
        }

        public void Draw(Material material, BoxedValueStore<string> parameters)
        {
            foreach (var pass in material.Begin(parameters))
            {
                pass.Apply();
                Draw();
            }
        }
    }
}
