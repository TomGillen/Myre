using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using Myre.Graphics.ModelViewer.Model;
using GalaSoft.MvvmLight.Command;
using Myre.Graphics.ModelViewer.Xna;

namespace Myre.Graphics.ModelViewer.ViewModel
{
    public class ModelPreviewViewModel
        : ViewModelBase
    {
        private SceneAdapter scene;
        private RelayCommand<XnaControlDrawnArgs> drawCommand;

        public ICommand DrawCommand
        {
            get { return drawCommand; }
        }
        
        public ModelPreviewViewModel(SceneAdapter scene)
            : base()
        {
            this.scene = scene;
            this.drawCommand = new RelayCommand<XnaControlDrawnArgs>(Draw);
        }

        private void Draw(XnaControlDrawnArgs e)
        {
            var resolution = GetElementPixelSize(e.Sender, (Vector)e.Sender.RenderSize);

            scene.Initialise();
            scene.SetResolution((int)resolution.Width, (int)resolution.Height);
            scene.Draw();
        }

        //public void SizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    var size = GetElementPixelSize(sender as UIElement, (Vector)e.NewSize);
        //    scene.SetResolution((int)size.Width, (int)size.Height);
        //}

        private Size GetElementPixelSize(UIElement element, Vector size)
        {
            Matrix transformToDevice;
            var source = PresentationSource.FromVisual(element);
            if (source != null)
                transformToDevice = source.CompositionTarget.TransformToDevice;
            else
                using (var s = new HwndSource(new HwndSourceParameters()))
                    transformToDevice = s.CompositionTarget.TransformToDevice;

            return (Size)transformToDevice.Transform(size);
        }
    }
}
