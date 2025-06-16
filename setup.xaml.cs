using CommunityToolkit.Maui.Alerts;

namespace LockBox;

public partial class Setup : ContentPage
{

    public Setup()
    {
        InitializeComponent();
    }

    private void OnPasswordToggleClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton toggleButton)
        {
            TogglePasswordVisibility(MasterPassword, toggleButton);
        }
    }
    private void TogglePasswordVisibility(Microsoft.Maui.Controls.Entry passwordEntry, ImageButton toggleButton)
    {
        // Toggle password visibility
        passwordEntry.IsPassword = !passwordEntry.IsPassword;

        // Update the button icon
        toggleButton.Source = new FontImageSource
        {
            Glyph = passwordEntry.IsPassword ? "\uE8F4" : "\uE8F5", // Replace with your icons
            FontFamily = "MaterialIcons"
        };
    }
    private async void OnSaveAndGoClicked(object sender, EventArgs e)
    {
        try
        {
            if (MasterPassword != null && !CredMan.ValidatePassword(password: MasterPassword.Text))
            {
                if (Application.Current?.Windows?[0]?.Page != null)
                {
                    await Application.Current.Windows[0].Page
                                             .DisplayAlert("Error", "Password does not meet minimum requirements", "Ok");
                }
                return;
            }
            Guid u = Guid.NewGuid();
            await CredMan.SaveCredentialsAsync(u.ToString(), password: MasterPassword.Text);
            if (Application.Current?.Windows?[0]?.Page != null)
            {
                // await Application.Current.Windows[0].Page.DisplaySnackbar("Set up with:" + u.ToString() + " and pw: " + MasterPassword.Text);
                Application.Current.Windows[0].Page = new ListScreen();
            }
        }
        catch (Exception ex)
        {
            if (Application.Current?.Windows?[0]?.Page != null)
            {
                await Application.Current.Windows[0].Page.DisplaySnackbar($"Error Saving Credentials: {ex.Message}");
            }
            return;
        }
    }
    private void OnTransferClicked(object sender, EventArgs e)
    {
        var mainWindow = Application.Current?.Windows?.FirstOrDefault();
        if (mainWindow != null)
        {
            mainWindow.Page = new TransferScreen();
        }
    }
}
