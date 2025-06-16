namespace LockBox
{
    public partial class App : Application
    {
        public static bool testOther = true;
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            const int Height = 800;
            const int Width = 400;
            bool HasCredentials = Task.Run(async () => await CredMan.AreCredentialsAvailableAsync()).GetAwaiter().GetResult();
            Window win;

            if (HasCredentials)
            {
                win = new Window(new LockScreen());
            }
            else
            {
                win = new Window(new Setup());
            }

            win.Height = Height;
            win.Width = Width;
            win.MaximumHeight = Height;
            win.MaximumWidth = Width;
            win.MinimumHeight = Height;
            win.MinimumWidth = Width;

            return win;
        }
    }
}
