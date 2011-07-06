using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Modules;
using Myre.Graphics.ModelViewer.Model;

namespace Myre.Graphics.ModelViewer
{
    public class DependancyModule
        : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<IStatusManager>().To<ApplicationStatus>().InSingletonScope();
            Kernel.Bind<SceneAdapter>().To<SceneAdapter>().InSingletonScope();
        }
    }
}
