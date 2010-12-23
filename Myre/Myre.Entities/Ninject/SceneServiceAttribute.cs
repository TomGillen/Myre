using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre.Entities
{
    /// <summary>
    /// An attribute which specifies that the target service should be retrieved from the scene.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class SceneServiceAttribute : Attribute
    {
    }
}
