/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:Myre.Graphics.ModelViewer.ViewModel"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
  
  OR (WPF only):
  
  xmlns:vm="clr-namespace:Myre.Graphics.ModelViewer.ViewModel"
  DataContext="{Binding Source={x:Static vm:ViewModelLocatorTemplate.ViewModelNameStatic}}"
*/

using Ninject;
namespace Myre.Graphics.ModelViewer.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// <para>
    /// Use the <strong>mvvmlocatorproperty</strong> snippet to add ViewModels
    /// to this locator.
    /// </para>
    /// <para>
    /// In Silverlight and WPF, place the ViewModelLocatorTemplate in the App.xaml resources:
    /// </para>
    /// <code>
    /// &lt;Application.Resources&gt;
    ///     &lt;vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:Myre.Graphics.ModelViewer.ViewModel"
    ///                                  x:Key="Locator" /&gt;
    /// &lt;/Application.Resources&gt;
    /// </code>
    /// <para>
    /// Then use:
    /// </para>
    /// <code>
    /// DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
    /// </code>
    /// <para>
    /// You can also use Blend to do all this with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// <para>
    /// In <strong>*WPF only*</strong> (and if databinding in Blend is not relevant), you can delete
    /// the Main property and bind to the ViewModelNameStatic property instead:
    /// </para>
    /// <code>
    /// xmlns:vm="clr-namespace:Myre.Graphics.ModelViewer.ViewModel"
    /// DataContext="{Binding Source={x:Static vm:ViewModelLocatorTemplate.ViewModelNameStatic}}"
    /// </code>
    /// </summary>
    public class ViewModelLocator
    {
        private static IKernel kernel = new StandardKernel(new DependancyModule());
        
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view models
            ////}
            ////else
            ////{
            ////    // Create run time view models
            ////}

            CreateMain();
        }

        /// <summary>
        /// Cleans up all the resources.
        /// </summary>
        public static void Cleanup()
        {
            ClearMain();
            ClearStatus();
            ClearPreview();
        }

        #region status
        private static StatusViewModel status;

        /// <summary>
        /// Gets the Status property.
        /// </summary>
        public static StatusViewModel StatusStatic
        {
            get
            {
                if (status == null)
                {
                    CreateStatus();
                }

                return status;
            }
        }

        /// <summary>
        /// Gets the Status property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public StatusViewModel Status
        {
            get
            {
                return StatusStatic;
            }
        }

        /// <summary>
        /// Provides a deterministic way to delete the Status property.
        /// </summary>
        public static void ClearStatus()
        {
            if (status != null)
            {
                status.Cleanup();
                status = null;
            }
        }

        /// <summary>
        /// Provides a deterministic way to create the Status property.
        /// </summary>
        public static void CreateStatus()
        {
            if (status == null)
            {
                status = kernel.Get<StatusViewModel>();
            }
        }
        #endregion

        #region main
        private static MainViewModel main;

        /// <summary>
        /// Gets the Main property.
        /// </summary>
        public static MainViewModel MainStatic
        {
            get
            {
                if (main == null)
                {
                    CreateMain();
                }

                return main;
            }
        }

        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel Main
        {
            get
            {
                return MainStatic;
            }
        }

        /// <summary>
        /// Provides a deterministic way to delete the Main property.
        /// </summary>
        public static void ClearMain()
        {
            if (main != null)
            {
                main.Cleanup();
                main = null;
            }
        }

        /// <summary>
        /// Provides a deterministic way to create the Main property.
        /// </summary>
        public static void CreateMain()
        {
            if (main == null)
            {
                main = kernel.Get<MainViewModel>();
            }
        }
        #endregion

        #region preview
        private static ModelPreviewViewModel preview;

        /// <summary>
        /// Gets the Preview property.
        /// </summary>
        public static ModelPreviewViewModel PreviewStatic
        {
            get
            {
                if (preview == null)
                {
                    CreatePreview();
                }

                return preview;
            }
        }

        /// <summary>
        /// Gets the Preview property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public ModelPreviewViewModel Preview
        {
            get
            {
                return PreviewStatic;
            }
        }

        /// <summary>
        /// Provides a deterministic way to delete the Preview property.
        /// </summary>
        public static void ClearPreview()
        {
            if (preview != null)
            {
                preview.Cleanup();
                preview = null;
            }
        }

        /// <summary>
        /// Provides a deterministic way to create the Preview property.
        /// </summary>
        public static void CreatePreview()
        {
            if (preview == null)
            {
                preview = kernel.Get<ModelPreviewViewModel>();
            }
        }
        #endregion
    }
}