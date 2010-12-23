using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre
{
    /// <summary>
    /// An object which can create copies of itself.
    /// </summary>
    /// <remarks>This is a replacement for the ICloneable interface, which does not exist in silverlight.</remarks>
    public interface ICopyable
    {
        object Copy();
    }
}