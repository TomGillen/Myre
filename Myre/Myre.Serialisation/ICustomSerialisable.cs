using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Myre.Serialisation
{
    public interface ICustomSerialisable
    {
        Dom.Node Serialise();
        void Deserialise(Dom.Node node);
    }
}
