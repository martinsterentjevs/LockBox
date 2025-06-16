using Foundation;

namespace Lockbox
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => LockBox.MauiProgram.CreateMauiApp();
    }
}
