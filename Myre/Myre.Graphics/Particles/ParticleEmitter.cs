using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Myre.Entities.Behaviours;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Ninject;
using Myre.Entities;
using Myre.Entities.Services;

namespace Myre.Graphics.Particles
{
    [DefaultManager(typeof(Manager))]
    public abstract class ParticleEmitter
        : Behaviour
    {
        private static Dictionary<ParticleSystemDescription, ParticleSystem> systemsDictionary = new Dictionary<ParticleSystemDescription, ParticleSystem>();
        private static List<ParticleSystem> systems = new List<ParticleSystem>();

        private IKernel kernel;
        private ParticleSystem system;
        private ParticleSystemDescription description;
        private bool dirty;

        public bool Enabled { get; set; }

        public ParticleType Type
        {
            get { return description.Type; }
            set
            {
                description.Type = value;
                dirty = true;
            }
        }

        public Texture2D Texture
        {
            get { return description.Texture; }
            set
            {
                description.Texture = value;
                dirty = true;
            }
        }

        public BlendState BlendState
        {
            get { return description.BlendState; }
            set
            {
                description.BlendState = value;
                dirty = true;
            }
        }

        public float Lifetime
        {
            get { return description.Lifetime; }
            set
            {
                description.Lifetime = value;
                dirty = true;
            }
        }

        public float EndLinearVelocity
        {
            get { return description.EndLinearVelocity; }
            set
            {
                description.EndLinearVelocity = value;
                dirty = true;
            }
        }
        
        public float EndScale
        {
            get { return description.EndScale; }
            set
            {
                description.EndScale = value;
                dirty = true;
            }
        }

        public Vector3 Gravity
        {
            get { return description.Gravity; }
            set
            {
                description.Gravity = value;
                dirty = true;
            }
        }

        protected ParticleSystem System
        {
            get { return system; }
        }

        protected bool Dirty
        {
            get { return dirty; }
            set { dirty = value; }
        }

        protected bool UsingUniqueSystem { get; set; }

        public ParticleEmitter(IKernel kernel)
        {
            this.kernel = kernel;
            this.dirty = true;
        }

        protected abstract void Update(float dt);

        protected void CreateParticleSystem()//bool unique)
        {
            //if (unique)
            //{
                system = kernel.Get<ParticleSystem>();
                system.Initialise(description);
                systems.Add(system);
            //}
            //else
            //{
            //    if (!systemsDictionary.TryGetValue(description, out system))
            //    {
            //        system = kernel.Get<ParticleSystem>();
            //        system.Initialise(description);

            //        systems.Add(system);
            //        systemsDictionary[description] = system;
            //    }
            //}

            //UsingUniqueSystem = unique;
        }

        public class Manager
            : BehaviourManager<ParticleEmitter>, IProcess
        {
            public bool IsComplete { get { return false; } }

            public Manager()
            {
            }

            public override void Initialise(Scene scene)
            {
                var processes = scene.GetService<ProcessService>();
                processes.Add(this);

                base.Initialise(scene);
            }

            public void Update(float elapsedTime)
            {
                foreach (var item in Behaviours)
                {
                    if (item.Enabled)
                        item.Update(elapsedTime);
                }

                foreach (var item in systems)
                {
                    item.Update(elapsedTime);
                }
            }

            public void Draw(Renderer renderer)
            {
                foreach (var item in systems)
                {
                    item.Draw(renderer.Data);
                }
            }
        }
    }
}
