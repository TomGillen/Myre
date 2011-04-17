using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Extensions;
using Myre.Entities;
using Microsoft.Xna.Framework;
using Myre.Entities.Behaviours;

namespace Myre.Physics
{
    public class Transform
        : Behaviour
    {
        private Property<Vector2> position;
        private Property<float> rotation;
        private Property<Matrix> transform;
        private Property<Matrix> inverseTransform;

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

        public Matrix TransformMatrix
        {
            get { return transform.Value; }
            set { transform.Value = value; }
        }

        public Matrix InverseTransformMatrix
        {
            get { return inverseTransform.Value; }
            set { inverseTransform.Value = value; }
        }

        public override void CreateProperties(Entity.InitialisationContext context)
        {
            this.position = context.CreateProperty<Vector2>(PropertyName.POSITION);
            this.rotation = context.CreateProperty<float>(PropertyName.ROTATION);
            this.transform = context.CreateProperty<Matrix>("transform_matrix");
            this.inverseTransform = context.CreateProperty<Matrix>("inverse_transform_matrix");

            base.CreateProperties(context);
        }

        public override void Initialise()
        {
            position.PropertyChanged += _ => CalculateTransform();
            rotation.PropertyChanged += _ => CalculateTransform();
        }

        public Vector2 ToWorldCoordinates(Vector2 point)
        {
            return Vector2.Transform(point, transform.Value);
        }

        public Vector2 ToLocalCoordinates(Vector2 point)
        {
            return Vector2.Transform(point, inverseTransform.Value);
        }

        private void CalculateTransform()
        {
            var pos = new Vector3(position.Value, 0);
            var rot = rotation.Value;

            Matrix temp1, temp2;
            Matrix.CreateRotationZ(rot, out temp1);
            Matrix.CreateTranslation(ref pos, out temp2);
            Matrix.Multiply(ref temp1, ref temp2, out temp1);
            Matrix.Invert(ref temp1, out temp2);

            transform.Value = temp1;
            inverseTransform.Value = temp2;
        }
    }
}
