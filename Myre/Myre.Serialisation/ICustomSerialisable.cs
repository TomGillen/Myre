using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre.Serialisation
{
    public interface ICustomSerialisable
    {
        Dom.Node Serialise();
    }
}
