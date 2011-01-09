using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Myre.Entities;
using Myre.Collections;
using Myre.Entities.Behaviours;
using ProtoBuf;

namespace Myre.Graphics
{
    [ProtoContract]
    public sealed class Camera
    {
        private Matrix view;
        private Matrix projection;
        private Matrix viewProjection;
        private Matrix inverseView;
        private Matrix inverseProjection;
        private Matrix inverseViewProjection;
        private float nearClip;
        private float farClip;
        private BoundingFrustum bounds;
        private bool isDirty = true;

        private Vector3[] frustumCorners = new Vector3[8];
        private Vector3[] farFrustumCorners = new Vector3[4];

        [ProtoMember(1)]
        public Matrix View
        {
            get { return view; }
            set
            {
                if (view != value)
                {
                    view = value;
                    isDirty = true;
                }
            }
        }

        [ProtoMember(2)]
        public Matrix Projection
        {
            get { return projection; }
            set 
            {
                projection = value; 
            }
        }

        public Matrix ViewProjection
        {
            get 
            {
                if (isDirty) Update();
                return viewProjection; 
            }
        }

        [ProtoMember(3)]
        public float NearClip
        {
            get { return nearClip; }
            set
            {
                if (nearClip != value)
                {
                    nearClip = value;
                    isDirty = true;
                }
            }
        }

        [ProtoMember(4)]
        public float FarClip
        {
            get { return farClip; }
            set
            {
                if (farClip != value)
                {
                    farClip = value;
                    isDirty = true;
                }
            }
        }

        public BoundingFrustum Bounds
        {
            get 
            {
                if (isDirty) Update();
                return bounds; 
            }
        }

        private void Update()
        {
            Matrix.Multiply(ref view, ref projection, out viewProjection);
            Matrix.Invert(ref view, out inverseView);
            Matrix.Invert(ref projection, out inverseProjection);
            Matrix.Invert(ref viewProjection, out inverseViewProjection);
            bounds = new BoundingFrustum(viewProjection);
            isDirty = false;
        }

        public void SetMetadata(BoxedValueStore<string> metadata)
        {
            metadata.Set("camera", this);
            metadata.Set("view", View);
            metadata.Set("projection", Projection);
            metadata.Set("viewprojection", ViewProjection);
            metadata.Set("inverseview", inverseView);
            metadata.Set("inverseprojection", inverseProjection);
            metadata.Set("inverseviewprojection", inverseViewProjection);
            metadata.Set("viewfrustum", Bounds);
            metadata.Set("nearclip", NearClip);
            metadata.Set("farclip", FarClip);
            metadata.Set("cameraposition", -view.Translation);

            bounds.GetCorners(frustumCorners);
            for (int i = 0; i < 4; i++)
                farFrustumCorners[i] = frustumCorners[i + 4];
            Vector3.Transform(farFrustumCorners, ref view, farFrustumCorners);
            metadata.Set("farfrustumcorners", farFrustumCorners);
        }
    }
}
