using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Threading.Tasks;

namespace Myre.Graphics.ModelViewer.Xna
{
    /// <summary>
    /// A static class containing components required for XNA interop.
    /// </summary>
    public static class XnaResources
    {
        private static GraphicsDeviceService graphicsService;
        private static ServiceContainer services;
        private static ContentBuilder contentBuilder;
        private static ContentManager contentManager;

        public static GraphicsDevice GraphicsDevice
        {
            get { return graphicsService.GraphicsDevice; }
        }

        public static GraphicsDeviceService GraphicsDeviceService
        {
            get { return graphicsService; }
        }

        public static ContentManager Content
        {
            get { return contentManager; }
        }

        public static IServiceProvider Services
        {
            get { return services; }
        }

        static XnaResources()
        {
            services = new ServiceContainer();
            contentBuilder = new ContentBuilder();
            contentManager = new ContentManager(services, contentBuilder.OutputDirectory);
        }

        /// <summary>
        /// Instantiates the graphics device.
        /// </summary>
        /// <remarks>
        /// Our model requires a graphics device instance before it can be constructed.
        /// However, the graphics device requires a window handle.
        /// This is the reverse of the relationship we would ideally want between the model and the view.
        /// </remarks>
        /// <param name="windowHandle"></param>
        public static void Initialise(IntPtr windowHandle)
        {
            if (graphicsService == null)
            {
                graphicsService = GraphicsDeviceService.AddRef(windowHandle);
                services.AddService<IGraphicsDeviceService>(graphicsService);
            }
        }

        /// <summary>
        /// Builds the specified content file.
        /// The content can then be loaded by the XnaResources.Content ContentManager instance.
        /// </summary>
        /// <param name="filename">The path to file to build.</param>
        /// <param name="name">The name of the content.</param>
        /// <param name="importer">The importer to use to import the file. <c>null</c> for the default importer.</param>
        /// <param name="processor">The processor to process the imported content. <c>null</c> for no processor.</param>
        /// <returns>The task which is building the content.</returns>
        public static Task<string> BuildContent(string filename, string name, string importer, string processor)
        {
            var task = new Task<string>(delegate()
                {
                    lock (contentBuilder)
                    {
                        contentBuilder.Clear();
                        contentBuilder.Add(filename, name, importer, processor);

                        var buildError = contentBuilder.Build();
                        return buildError;
                    }
                });

            task.Start();

            return task;
        }
    }
}
