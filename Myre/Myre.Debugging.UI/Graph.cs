 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Myre.Debugging.Statistics
{
    public class Graph
    {
        float max;
        float[] data;
        VertexPositionColor[] transformedData;
        VertexBuffer vertices;
        bool dirty;
        Color colour;
        BasicEffect effect;
        float screenWidth, screenHeight;
        Rectangle previousArea;

        public Color Colour
        {
            get { return colour; }
            set
            {
                if (colour != value)
                {
                    colour = value;
                    dirty = true;
                }
            }
        }

        public Graph(GraphicsDevice device, int resolution)
        {
            vertices = new VertexBuffer(device, typeof(VertexPositionColor), resolution, BufferUsage.WriteOnly);
            data = new float[resolution];
            transformedData = new VertexPositionColor[resolution];
            colour = Color.Red;
            //effect = Content.Load<Effect>(game, "Graph");

            //var pp = device.PresentationParameters;
#if XNA_3_1
            effect = new BasicEffect(device, new EffectPool());
#else
            effect = new BasicEffect(device);
#endif
            effect.LightingEnabled = false;
            effect.VertexColorEnabled = true;
            effect.World = Matrix.Identity;
            effect.View = Matrix.Identity;
            effect.Projection = Matrix.Identity; //Matrix.CreateOrthographic(pp.BackBufferWidth, pp.BackBufferHeight, 0, 1);

            dirty = true;
        }

        public void Add(float value)
        {
            for (int i = 0; i < data.Length - 1; i++)
                data[i] = data[i + 1];

            if (value > max)
            {
                float scale = max / value;
                for (int i = 0; i < data.Length - 1; i++)
                    data[i] = data[i] * scale;
                data[data.Length - 1] = 1;
                max = value;
            }
            else
                data[data.Length - 1] = value / max;

            dirty = true;
        }

        public void Draw(Rectangle area)
        {
            var device = vertices.GraphicsDevice;
            var pp = device.PresentationParameters;

            if (dirty || area != previousArea || screenHeight != pp.BackBufferHeight || screenWidth != pp.BackBufferWidth)
            {
                screenHeight = pp.BackBufferHeight;
                screenWidth = pp.BackBufferWidth;

                float x = (area.X / screenWidth) * 2 - 1;
                float y = -((area.Y / screenHeight) * 2 - 1);
                float width = (area.Width / screenWidth) * 2;
                float height = (area.Height / screenHeight) * 2;

                for (int i = 0; i < data.Length; i++)
                {
                    var position = new Vector3(
                            x + (i / (float)(data.Length - 1)) * width,
                            (y - height) + data[i] * height,
                            0);
                    transformedData[i] = new VertexPositionColor(position, colour);
                }

                vertices.SetData<VertexPositionColor>(transformedData);

                dirty = false;
                previousArea = area;
            }

#if XNA_3_1
            effect.Begin();
            effect.Techniques[0].Passes[0].Begin();
            device.Vertices[0].SetSource(vertices, 0, VertexPositionColor.SizeInBytes);
            device.DrawPrimitives(PrimitiveType.LineStrip, 0, data.Length - 1);
            effect.Techniques[0].Passes[0].End();
            effect.End();
#else
            effect.Techniques[0].Passes[0].Apply();
            device.SetVertexBuffer(vertices);
            device.DrawPrimitives(PrimitiveType.LineStrip, 0, data.Length - 1);
#endif
        }
    }
}
