using CommunityToolkit.Maui.Alerts;

namespace LockBox;

public partial class LockScreen : ContentPage
{
    public LockScreen()
    {
        InitializeComponent();
    }

    private void OnPasswordToggleClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton toggleButton)
        {
            TogglePasswordVisibility(UserPassword, toggleButton);
        }
    }

    private void TogglePasswordVisibility(Microsoft.Maui.Controls.Entry passwordEntry, ImageButton toggleButton)
    {
        passwordEntry.IsPassword = !passwordEntry.IsPassword;
        toggleButton.Source = new FontImageSource
        {
            Glyph = passwordEntry.IsPassword ? "\uE8F4" : "\uE8F5",
            FontFamily = "MaterialIcons"
        };
    }

    private async void Login(object sender, EventArgs e)
    {
        if (IsPasswordEmpty())
        {
            await ShowAlert("Error", "Password can't be empty");
            return;
        }

        if (!await CredMan.AreCredentialsAvailableAsync())
        {
            await ShowAlert("Error", "No credentials found. Please reset the application.");
            return;
        }

        await ValidatePassword();
    }

    private bool IsPasswordEmpty() => string.IsNullOrEmpty(UserPassword?.Text);

    private async Task ShowAlert(string title, string message)
    {
        var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (mainPage != null)
        {
            await mainPage.DisplayAlert(title, message, "OK");
        }
    }

    private async Task ValidatePassword()
    {
        try
        {
            if (await CredMan.IsPasswordCorrectAsync(UserPassword.Text))
            {
                var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                {
                    await mainPage.DisplaySnackbar("Success");
                    if (Application.Current?.Windows[0] != null)
                    {
                        Application.Current.Windows[0].Page = new ListScreen();
                    }
                }
                else
                {
                    await ShowAlert("Error", "Main page not found");
                }
            }
            else
            {
                await ShowAlert("Error", "Incorrect Password");
            }
        }
        catch (Exception err)
        {
            await ShowAlert("Error", err.Message);
        }
    }

    private void Delete(object sender, EventArgs e)
    {
        CredMan.DeleteCredentialsAsync().ContinueWith(async task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                {
                    await mainPage.DisplaySnackbar("Credentials deleted successfully.");
                    await mainPage.Navigation.PushAsync(new Setup());
                }
            }
            else
            {
                await ShowAlert("Error", "Failed to delete credentials.");
            }
        });
    }
}
