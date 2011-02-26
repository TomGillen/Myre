using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities;
using Microsoft.Xna.Framework;
using Myre.Entities.Behaviours;
using Ninject;
using Myre.Entities.Services;

namespace Myre.Physics
{
    [DefaultManager(typeof(Manager))]
    public class DynamicPhysics
        : Behaviour
    {
        private Property<Vector2> position;
        private Property<float> rotation;
        private Property<float> mass;
        private Property<float> inertiaTensor;
        private Property<Vector2> linearVelocity;
        private Property<float> angularVelocity;
        private Property<float> timeMultiplier;

        private Vector2 force;
        private float torque;
        private Vector2 linearVelocityBias;
        private float angularVelocityBias;

        #region temp
        private Vector2 r;
        #endregion
        
        public Vector2 Position
        {
            get { return position.Value; }
            set { position.Value = value; }
        }

        public float Rotation
        {
            get { return rotation.Value; }
            set { rotation.Value = value; }
        }

        public float Mass
        {
            get { return mass.Value; }
            set { mass.Value = value; }
        }

        public float InertiaTensor
        {
            get { return inertiaTensor.Value; }
            set { inertiaTensor.Value = value; }
        }

        public Vector2 LinearVelocity
        {
            get { return linearVelocity.Value; }
            set { linearVelocity.Value = value; }
        }

        public float AngularVelocity
        {
            get { return angularVelocity.Value; }
            set { angularVelocity.Value = value; }
        }

        public float TimeMultiplier
        {
            get { return timeMultiplier.Value; }
            set { timeMultiplier.Value = value; }
        }

        public const String POSITION = "position";
        public const String ROTATION = "rotation";
        public const String MASS = "mass";
        public const String INERTIA_TENSOR = "inertia_tensor";
        public const String LINEAR_VELOCITY = "linear_velocity";
        public const String ANGULAR_VELOCITY = "angular_velocity";
        public const String TIME_MULTIPLIER = "time_multiplier";

        public override void CreateProperties(Entity.InitialisationContext context)
        {
            this.position = context.CreateProperty<Vector2>(POSITION);
            this.rotation = context.CreateProperty<float>(ROTATION);
            this.mass = context.CreateProperty<float>(MASS);
            this.inertiaTensor = context.CreateProperty<float>(INERTIA_TENSOR);
            this.linearVelocity = context.CreateProperty<Vector2>(LINEAR_VELOCITY);
            this.angularVelocity = context.CreateProperty<float>(ANGULAR_VELOCITY);
            this.timeMultiplier = context.CreateProperty<float>(TIME_MULTIPLIER);

            base.CreateProperties(context);
        }

        public Vector2 GetVelocityAtOffset(Vector2 worldOffset)
        {
            var value = linearVelocity.Value;
            value.X += -angularVelocity.Value * worldOffset.Y;
            value.Y += angularVelocity.Value * worldOffset.X;

            return value;
        }

        internal Vector2 GetVelocityBiasAtOffset(Vector2 worldOffset)
        {
            var value = linearVelocityBias;
            value.X += -angularVelocityBias * worldOffset.Y;
            value.Y += angularVelocityBias * worldOffset.X;

            return value;
        }

        public void ApplyForce(Vector2 force, Vector2 worldPosition)
        {
            Vector2.Add(ref this.force, ref force, out this.force);
            var pos = position.Value;
            Vector2.Subtract(ref worldPosition, ref pos, out r);
            torque += r.X * force.X - r.Y * force.Y;
        }

        public void ApplyForceAtOffset(Vector2 force, Vector2 worldOffset)
        {
            Vector2.Add(ref this.force, ref force, out this.force);
            torque += worldOffset.X * force.X - worldOffset.Y * force.Y;
        }

        public void ApplyForce(Vector2 force)
        {
            this.force += force;
        }

        public void ApplyImpulse(Vector2 impulse, Vector2 worldPosition)
        {
            var pos = position.Value;
            Vector2.Subtract(ref worldPosition, ref pos, out r);
            ApplyImpulseAtOffset(impulse, r);
        }

        public void ApplyImpulseAtOffset(Vector2 impulse, Vector2 worldOffset)
        {
            impulse /= Mass;
            linearVelocity.Value += impulse;
            angularVelocity.Value += (worldOffset.X * impulse.Y - impulse.Y * worldOffset.X) / InertiaTensor;
        }

        public void ApplyImpulse(Vector2 impulse)
        {
            linearVelocity.Value += impulse / Mass;
        }

        internal void ApplyBiasImpulse(Vector2 impulse, Vector2 worldPosition)
        {
            var pos = position.Value;
            Vector2.Subtract(ref worldPosition, ref pos, out r);
            ApplyImpulseAtOffset(impulse, r);
        }

        internal void ApplyBiasImpulseAtOffset(Vector2 impulse, Vector2 worldOffset)
        {
            impulse /= Mass;
            linearVelocity.Value += impulse;
            angularVelocityBias += (worldOffset.X * impulse.Y - impulse.Y * worldOffset.X) / InertiaTensor;
        }

        public class Manager
            : BehaviourManager<DynamicPhysics>, IProcess
        {
            public bool IsComplete { get { return false; } }

            public Manager(IProcessService processes)
            {
                processes.Add(this);
            }

            public void Update(float time)
            {
                for (int i = 0; i < Behaviours.Count; i++)
                {
                    Integrate(Behaviours[i], time * Behaviours[i].TimeMultiplier);
                }
            }

            private void Integrate(DynamicPhysics body, float time)
            {
                body.linearVelocity.Value += body.force / body.Mass;
                body.force = Vector2.Zero;

                body.angularVelocity.Value += body.torque / body.InertiaTensor;
                body.torque = 0;

                body.position.Value += (body.linearVelocity.Value + body.linearVelocityBias) * time;
                body.rotation.Value += (body.angularVelocity.Value + body.angularVelocityBias) * time;

                body.linearVelocityBias = Vector2.Zero;
                body.angularVelocityBias = 0;
            }
        }
    }
}
