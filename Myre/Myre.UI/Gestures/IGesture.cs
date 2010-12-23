using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.UI.InputDevices;

namespace Myre.UI.Gestures
{
    public interface IGesture
    {
        ICollection<int> BlockedInputs { get; }
        Type DeviceType { get; }
        bool AlwaysEvaluate { get; }
        bool Test(IInputDevice device);
    }

    public abstract class Gesture<Device>
        : IGesture
        where Device : IInputDevice
    {
        private static Type deviceType = typeof(Device);
        private List<int> blockedInputs = new List<int>();

        public ICollection<int> BlockedInputs
        {
            get { return blockedInputs; }
        }

        public Type DeviceType
        {
            get { return deviceType; }
        }

        public bool AlwaysEvaluate
        {
            get;
            private set;
        }

        public Gesture(bool alwaysEvaluate)
        {
            AlwaysEvaluate = alwaysEvaluate;
        }

        public abstract bool Test(Device device);

        bool IGesture.Test(IInputDevice device)
        {
            return Test((Device)device);
        }
    }
}