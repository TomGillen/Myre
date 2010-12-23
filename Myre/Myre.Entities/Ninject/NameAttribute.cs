using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre.Entities
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class NameAttribute : Attribute
    {
        public string Name { get; private set; }

        public NameAttribute(string name)
        {
            this.Name = name;
        }
    }
}
