using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Services;
using System.Collections.ObjectModel;
using Myre.Entities;
using Myre.Collections;
using Microsoft.Xna.Framework;

namespace Myre.Physics
{
    public interface IForceProvider
    {
        void Update(float elapsedTime);
    }

    public interface IIntegrator
    {
        void UpdateVelocity(float elapsedTime);
        void UpdatePosition(float elapsedTime);
    }

    public interface IActivityManager
    {
        void UpdateActivityStatus(float elapsedTime, float linearVelocityThreshold, float angularVelocityThreshold);
        void FreezeSleepingObjects();
    }

    public interface ICollisionResolver
    {
        void Update(float elapsedTime, float allowedPenetration, float biasFactor, int iterations);
    }

    public class PhysicsEngine
        : Service
    {
        private ReadOnlyCollection<IForceProvider> forceProviders;
        private ReadOnlyCollection<IIntegrator> integrators;
        private ReadOnlyCollection<IActivityManager> activityManagers;
        private ReadOnlyCollection<ICollisionResolver> collisionResolvers;

        private Box<float> allowedPenetration;
        private Box<float> biasFactor;
        private Box<int> iterations;
        private Box<float> linearVelocitySleepThreshold;
        private Box<float> angularVelocitySleepThreshold;

        public float AllowedPenetration
        {
            get { return allowedPenetration.Value; }
            set { allowedPenetration.Value = value; }
        }

        public float BiasFactor
        {
            get { return biasFactor.Value; }
            set { biasFactor.Value = value; }
        }

        public int Iterations 
        {
            get { return iterations.Value; }
            set { iterations.Value = value; }
        }

        public float LinearVelocitySleepThreshold
        {
            get { return linearVelocitySleepThreshold.Value; }
            set { linearVelocitySleepThreshold.Value = value; }
        }

        public float AngularVelocitySleepThreshold
        {
            get { return angularVelocitySleepThreshold.Value; }
            set { angularVelocitySleepThreshold.Value = value; }
        }

        public PhysicsEngine(Scene scene)
        {
            forceProviders = scene.FindManagers<IForceProvider>();
            integrators = scene.FindManagers<IIntegrator>();
            activityManagers = scene.FindManagers<IActivityManager>();
            collisionResolvers = scene.FindManagers<ICollisionResolver>();

            allowedPenetration = new Box<float>();
            biasFactor = new Box<float>();
            iterations = new Box<int>();
            linearVelocitySleepThreshold = new Box<float>();
            angularVelocitySleepThreshold = new Box<float>();

            AllowedPenetration = 2;
            BiasFactor = 0.1f;
            Iterations = 15;
            LinearVelocitySleepThreshold = 0.5f;
            AngularVelocitySleepThreshold = MathHelper.TwoPi * 0.025f;
        }

        public void BindSettings(Box<float> allowedPenetration, Box<float> biasFactor, Box<int> iterations, Box<float> linearThreshold, Box<float> angularThreshold)
        {
            Assert.ArgumentNotNull("allowedPenetration", allowedPenetration);
            Assert.ArgumentNotNull("biasFactor", biasFactor);
            Assert.ArgumentNotNull("iterations", iterations);
            Assert.ArgumentNotNull("linearThreshold", linearThreshold);
            Assert.ArgumentNotNull("angularThreshold", angularThreshold);

            this.allowedPenetration = allowedPenetration;
            this.biasFactor = biasFactor;
            this.iterations = iterations;
            this.linearVelocitySleepThreshold = linearThreshold;
            this.angularVelocitySleepThreshold = angularThreshold;
        }

        public override void Update(float elapsedTime)
        {
            for (int i = 0; i < forceProviders.Count; i++)
                forceProviders[i].Update(elapsedTime);

            for (int i = 0; i < integrators.Count; i++)
                integrators[i].UpdateVelocity(elapsedTime);

            for (int i = 0; i < activityManagers.Count; i++)
                activityManagers[i].UpdateActivityStatus(elapsedTime, LinearVelocitySleepThreshold, AngularVelocitySleepThreshold);

            for (int i = 0; i < collisionResolvers.Count; i++)
                collisionResolvers[i].Update(elapsedTime, AllowedPenetration, BiasFactor, Iterations);

            for (int i = 0; i < activityManagers.Count; i++)
                activityManagers[i].FreezeSleepingObjects();

            for (int i = 0; i < integrators.Count; i++)
                integrators[i].UpdatePosition(elapsedTime);
            
            base.Update(elapsedTime);
        }
    }
}
