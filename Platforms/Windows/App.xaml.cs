using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Lockbox.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        public App()
        {
            this.InitializeComponent();
        }

        protected override MauiApp CreateMauiApp() => LockBox.MauiProgram.CreateMauiApp();

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);

            var window = Microsoft.Maui.Controls.Application.Current!.Windows[0].Handler.PlatformView as Microsoft.UI.Xaml.Window;
            if (window != null)
            {
                window.ExtendsContentIntoTitleBar = true;
                window.SetTitleBar(null);
                var appWindow = window.AppWindow;
                if (appWindow != null)
                {
                    appWindow.SetPresenter(AppWindowPresenterKind.CompactOverlay);
                }
            }
        }
    }

}
