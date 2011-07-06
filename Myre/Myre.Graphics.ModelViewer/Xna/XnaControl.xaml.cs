using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.ComponentModel;
using Microsoft.Xna.Framework.Graphics;

namespace Myre.Graphics.ModelViewer.Xna
{
    public class XnaControlDrawnArgs
        : EventArgs
    {
        public XnaControl Sender { get; private set; }
        public GraphicsDevice Device { get; private set; }

        public XnaControlDrawnArgs(XnaControl sender, GraphicsDevice device)
        {
            Sender = sender;
            Device = device;
        }
    }

    /// <summary>
    /// Interaction logic for XnaControl.xaml
    /// </summary>
    public partial class XnaControl : UserControl
    {
        private XnaImageSource imageSource;

        public event EventHandler<XnaControlDrawnArgs> DrawFunction;

        public XnaControl()
        {
            InitializeComponent();

            // hook up an event to fire when the control has finished loading
            Loaded += new RoutedEventHandler(XnaControl_Loaded);
        }

        ~XnaControl()
        {
            if (imageSource != null)
                imageSource.Dispose();
        }

        void XnaControl_Loaded(object sender, RoutedEventArgs e)
        {
            // if we're not in design mode, initialize the graphics device
            if (DesignerProperties.GetIsInDesignMode(this) == false)
            {
                InitializeGraphicsDevice();
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            // if we're not in design mode, recreate the 
            // image source for the new size
            if (DesignerProperties.GetIsInDesignMode(this) == false &&
                XnaResources.GraphicsDeviceService != null)
            {
                // recreate the image source
                imageSource.Dispose();
                imageSource = new XnaImageSource(
                    XnaResources.GraphicsDevice, (int)ActualWidth, (int)ActualHeight);
                rootImage.Source = imageSource.WriteableBitmap;
            }

            base.OnRenderSizeChanged(sizeInfo);
        }

        private void InitializeGraphicsDevice()
        {
            if (imageSource == null)
            {
                var handle = (PresentationSource.FromVisual(this) as HwndSource).Handle;
                XnaResources.Initialise(handle);

                // create the image source
                imageSource = new XnaImageSource(
                    XnaResources.GraphicsDevice, (int)ActualWidth, (int)ActualHeight);
                rootImage.Source = imageSource.WriteableBitmap;

                // hook the rendering event
                CompositionTarget.Rendering += CompositionTarget_Rendering;
            }
        }

        /// <summary>
        /// Draws the control and allows subclasses to override 
        /// the default behaviour of delegating the rendering.
        /// </summary>
        protected virtual void Render()
        {
            // invoke the draw command so someone will draw something pretty
            if (DrawFunction != null)
            {
                var args = new XnaControlDrawnArgs(this, XnaResources.GraphicsDevice);
                DrawFunction(this, args);
            }
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            // set the image source render target
            XnaResources.GraphicsDevice.SetRenderTarget(imageSource.RenderTarget);

            // allow the control to draw
            Render();

            // unset the render target
            XnaResources.GraphicsDevice.SetRenderTarget(null);

            // commit the changes to the image source
            imageSource.Commit();
        }
    }
}
