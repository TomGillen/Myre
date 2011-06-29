// Based upon the Particles3D sample on AppHub.
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Myre.Graphics.Materials;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Myre.Graphics.Particles
{
    /// <summary>
    /// A class which manages and updating and rendering of particles.
    /// </summary>
    public class ParticleSystem
    {

        private GraphicsDevice device;
        private Material material;
        private EffectParameter currentTimeParameter;
        private EffectParameter viewportScaleParameter;

        private ParticleVertex[] particles;
        private DynamicVertexBuffer vertices;
        private IndexBuffer indices;

        private int active;
        private int newlyCreated;
        private int free;
        private int finished;

        private float time;
        private int frameCounter;

        private bool dirty;

        /// <summary>
        /// Gets the settings for this particle system.
        /// </summary>
        public ParticleSystemDescription Description { get; private set; }

        /// <summary>
        /// Gets or sets the world transformation matrix to apply to this <see cref="ParticleSystem"/>s' particles.
        /// </summary>
        public Matrix Transform { get; set; }

        /// <summary>
        /// Gets the maximum number of particles this system can maintain at a time.
        /// </summary>
        public int Capacity { get; private set; }

        /// <summary>
        /// Gets the number of active particles.
        /// </summary>
        public int ActiveCount
        {
            get
            {
                if (active < newlyCreated)
                    return newlyCreated - active;

                return newlyCreated + (Capacity - active);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticleSystem"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        public ParticleSystem(GraphicsDevice device)
        {
            this.device = device;
            this.material = new Material(Content.Load<Effect>("ParticleSystem").Clone());
            this.Capacity = 5;
        }

        /// <summary>
        /// Initialises this <see cref="ParticleSystem"/> instance.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="blendMode">The blend mode.</param>
        public void Initialise(ParticleSystemDescription description)
        {
            this.Description = description;

            material.Parameters["Texture"].SetValue(description.Texture);
            material.Parameters["Lifetime"].SetValue(description.Lifetime);
            material.Parameters["EndVelocity"].SetValue(description.EndLinearVelocity);
            material.Parameters["EndScale"].SetValue(description.EndScale);
            material.Parameters["Gravity"].SetValue(description.Gravity);

            currentTimeParameter = material.Parameters["Time"];
            viewportScaleParameter = material.Parameters["ViewportScale"];

            InitialiseBuffer();
        }

        /// <summary>
        /// Increases the capacity of this system by the specified amount.
        /// </summary>
        /// <param name="size">The amount by which to increase the capacity.</param>
        public void GrowCapacity(int size)
        {
            Capacity += size;
            dirty = true;
        }

        /// <summary>
        /// Spawns a new particle.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="velocity">The velocity.</param>
        /// <param name="angularVelocity">The angular velocity.</param>
        /// <param name="size">The size.</param>
        /// <param name="lifetimeScale">The lifetime scale. This should be between 0 and 1.</param>
        /// <param name="startColour">The start colour.</param>
        /// <param name="endColour">The end colour.</param>
        public void Spawn(Vector3 position, Vector3 velocity, float angularVelocity, float size, float lifetimeScale, Color startColour, Color endColour)
        {
            if (dirty)
                InitialiseBuffer();

            // exit if we have run out of capacity
            int nextFreeParticle = (free + 1) % Capacity;
            if (nextFreeParticle == finished)
                return;

            // write data into buffer
            for (int i = 0; i < 4; i++)
            {
                particles[free * 4 + i].Position = position;
                particles[free * 4 + i].Velocity = new Vector4(velocity, angularVelocity);
                particles[free * 4 + i].Scales = new HalfVector2(size, lifetimeScale > 0 ? 1f / lifetimeScale : 1);
                particles[free * 4 + i].StartColour = startColour;
                particles[free * 4 + i].EndColour = endColour;
                particles[free * 4 + i].Time = time;
            }

            free = nextFreeParticle;
        }

        public void Update(float dt)
        {
            if (dirty)
                InitialiseBuffer();

            time += dt;

            RetireActiveParticles();
            FreeRetiredParticles();

            bool noActiveParticles = active == free;
            bool noFinishedParticles = finished == active;

            if (noActiveParticles && noFinishedParticles)
            {
                time = 0;
                frameCounter = 0;
            }
        }

        public void Draw(RendererMetadata data)
        {
            Debug.Assert(Description.Texture != null, "Particle systems must be initialised before they can be drawn.");
            //Debug.WriteLine(string.Format("retired: {0}, active: {1}, new: {2}, free: {3}", finished, active, newlyCreated, free));

            if (dirty)
                InitialiseBuffer();

            if (newlyCreated != free)
                AddNewParticlesToVertexBuffer();

            if (vertices.IsContentLost)
                vertices.SetData(particles);

            // If there are any active particles, draw them now!
            if (active != free)
            {
                device.BlendState = Description.BlendState;
                device.DepthStencilState = DepthStencilState.DepthRead;

                // Set an effect parameter describing the viewport size. This is
                // needed to convert particle sizes into screen space point sizes.
                viewportScaleParameter.SetValue(new Vector2(0.5f / data.Get<Viewport>("viewport").Value.AspectRatio, -0.5f));

                // Set an effect parameter describing the current time. All the vertex
                // shader particle animation is keyed off this value.
                currentTimeParameter.SetValue(time);

                data.Set<Matrix>("world", Transform);

                // Set the particle vertex and index buffer.
                device.SetVertexBuffer(vertices);
                device.Indices = indices;

                SelectParticleType();
                
                // Activate the particle effect.
                foreach (EffectPass pass in material.Begin(data))
                {
                    pass.Apply();

                    // work arround for bug in xna 4.0
                    device.SamplerStates[0] = SamplerState.PointClamp;
                    device.SamplerStates[1] = SamplerState.PointClamp;
                    device.SamplerStates[2] = SamplerState.PointClamp;

                    if (active < free)
                    {
                        // If the active particles are all in one consecutive range,
                        // we can draw them all in a single call.
                        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
                                                     active * 4, (free - active) * 4,
                                                     active * 6, (free - active) * 2);
                    }
                    else
                    {
                        // If the active particle range wraps past the end of the queue
                        // back to the start, we must split them over two draw calls.
                        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
                                                     active * 4, (Capacity - active) * 4,
                                                     active * 6, (Capacity - active) * 2);

                        if (free > 0)
                        {
                            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
                                                         0, free * 4,
                                                         0, free * 2);
                        }
                    }
                }

                // Reset some of the renderstates that we changed,
                // so as not to mess up any other subsequent drawing.
                device.DepthStencilState = DepthStencilState.Default;
                device.BlendState = BlendState.AlphaBlend;
            }

            frameCounter++;
        }

        private void SelectParticleType()
        {
            switch (Description.Type)
            {
                case ParticleType.Hard:
                    material.CurrentTechnique = material.Techniques["Hard"];
                    break;
                case ParticleType.Soft:
                    material.CurrentTechnique = material.Techniques["Soft"];
                    break;
                default:
                    break;
            }
        }

        private void InitialiseBuffer()
        {
            // dispose exiting buffers
            if (this.vertices != null)
                this.vertices.Dispose();
            if (this.indices != null)
                this.indices.Dispose();

            // create new vertex buffer
            this.vertices = new DynamicVertexBuffer(device, ParticleVertex.VertexDeclaration, Capacity * 4, BufferUsage.WriteOnly);
            
            // set up quad corners
            var particles = new ParticleVertex[Capacity * 4];
            for (int i = 0; i < Capacity; i++)
            {
                particles[i * 4 + 0].Corner = new Short2(-1, -1);
                particles[i * 4 + 1].Corner = new Short2(1, -1);
                particles[i * 4 + 2].Corner = new Short2(1, 1);
                particles[i * 4 + 3].Corner = new Short2(-1, 1);
            }

            // copy over any exiting active particles
            int j = 0;
            for (int i = active *4; i != free * 4; i = (i + 1) % particles.Length)
            {
                particles[j] = this.particles[i];
                j++;
            }

            // swap array over to the new larger array
            this.particles = particles;

            // create new index buffer
            ushort[] indices = new ushort[Capacity * 6];
            for (int i = 0; i < Capacity; i++)
            {
                indices[i * 6 + 0] = (ushort)(i * 4 + 0);
                indices[i * 6 + 1] = (ushort)(i * 4 + 1);
                indices[i * 6 + 2] = (ushort)(i * 4 + 2);

                indices[i * 6 + 3] = (ushort)(i * 4 + 0);
                indices[i * 6 + 4] = (ushort)(i * 4 + 2);
                indices[i * 6 + 5] = (ushort)(i * 4 + 3);
            }

            this.indices = new IndexBuffer(device, typeof(ushort), indices.Length, BufferUsage.WriteOnly);
            this.indices.SetData(indices);

            dirty = false;
        }

        /// <summary>
        /// Helper for checking when active particles have reached the end of
        /// their life. It moves old particles from the active area of the queue
        /// to the retired section.
        /// </summary>
        // Modified from Particles3D sample
        private void RetireActiveParticles()
        {
            while (active != free)
            {
                // Is this particle old enough to retire?
                // We multiply the active particle index by four, because each
                // particle consists of a quad that is made up of four vertices.
                float particleAge = time - particles[active * 4].Time;

                if (particleAge < Description.Lifetime)
                    break;

                // Remember the time at which we retired this particle.
                particles[active * 4].Time = frameCounter;

                // Move the particle from the active to the retired queue.
                active = (active + 1) % Capacity;
            }
        }


        /// <summary>
        /// Helper for checking when retired particles have been kept around long
        /// enough that we can be sure the GPU is no longer using them. It moves
        /// old particles from the retired area of the queue to the free section.
        /// </summary>
        // Modified from Particles3D sample
        private void FreeRetiredParticles()
        {
            while (finished != active)
            {
                // Has this particle been unused long enough that
                // the GPU is sure to be finished with it?
                // We multiply the retired particle index by four, because each
                // particle consists of a quad that is made up of four vertices.
                int age = frameCounter - (int)particles[finished * 4].Time;

                // The GPU is never supposed to get more than 2 frames behind the CPU.
                // We add 1 to that, just to be safe in case of buggy drivers that
                // might bend the rules and let the GPU get further behind.
                if (age < 3)
                    break;

                // Move the particle from the retired to the free queue.
                finished = (finished + 1) % Capacity;
            }
        }

        /// <summary>
        /// Helper for uploading new particles from our managed
        /// array to the GPU vertex buffer.
        /// </summary>
        // Modified from Particles3D sample
        void AddNewParticlesToVertexBuffer()
        {
            int stride = ParticleVertex.SizeInBytes;

            if (newlyCreated < free)
            {
                // If the new particles are all in one consecutive range,
                // we can upload them all in a single call.
                vertices.SetData(newlyCreated * stride * 4, particles,
                                 newlyCreated * 4, (free - newlyCreated) * 4,
                                 stride, SetDataOptions.NoOverwrite);
            }
            else
            {
                // If the new particle range wraps past the end of the queue
                // back to the start, we must split them over two upload calls.
                vertices.SetData(newlyCreated * stride * 4, particles,
                                 newlyCreated * 4,
                                 (Capacity - newlyCreated) * 4,
                                 stride, SetDataOptions.NoOverwrite);

                if (free > 0)
                {
                    vertices.SetData(0, particles,
                                     0, free * 4,
                                     stride, SetDataOptions.NoOverwrite);
                }
            }

            // Move the particles we just uploaded from the new to the active queue.
            newlyCreated = free;
        }
    }
}
