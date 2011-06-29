using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace Myre.Graphics
{
    public static class Content
    {
        private static ResourceContentManager manager;

        public static void Initialise(IServiceProvider services)
        {
#if WINDOWS
            manager = new ResourceContentManager(services, x86Resources.ResourceManager);
#else
            manager = new ResourceContentManager(services, XboxResources.ResourceManager);
#endif
        }

        public static T Load<T>(string resource)
        {
            if (manager == null)
                throw new Exception("Myre.Graphics.Content.Initialise() must be called before Myre.Graphics can load its' resources.");

            return manager.Load<T>(resource);
        }
    }
}
