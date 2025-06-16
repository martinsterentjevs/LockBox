using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Controls;
using System.Web;

namespace LockBox;

public partial class AddScreen : ContentPage
{
    public AddScreen()
    {
        InitializeComponent();
        AlgorithmPicker.SelectedIndex = 0;
        if (DeviceInfo.Idiom == DeviceIdiom.Desktop)
        {
            MFA_Notice.Text = "To add 2FA for this account you may need to switch to a phone";
        }
        else if (DeviceInfo.Idiom == DeviceIdiom.Phone)
        {
            MFA_Notice.Text = "To add 2FA for this account press the 'Scan' button to scan the code";
        }
        else
        {
            MFA_Notice.Text = "";
        }
    }

    private void GeneratePassword_Clicked(object sender, EventArgs e)
    {
        int passwordLength = (int)PasswordLengthSlider.Value;
        Pass.Text = PasswordGenerator.GeneratePassword(passwordLength);
    }

    private async void Back_Clicked(object sender, EventArgs e)
    {
        var mainWindow = Application.Current?.Windows.FirstOrDefault();
        if (mainWindow != null)
        {
            await Dispatcher.DispatchAsync(() => mainWindow.Page = new ListScreen());
        }
    }

    private async void Save_Clicked(object sender, EventArgs e)
    {
        string title = Serv.Text;
        string user = User.Text;
        string pw = Pass.Text;
        string mfa = MFA.Text;
        string algorithm = AlgorithmPicker.SelectedItem?.ToString() ?? "SHA1";

        try
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(user))
            {
                throw new Exception("Please fill in 'Service name' and 'Username/E-mail' fields");
            }

            // Check for existing entries with the same name and username
            var entries = await ServiceEntry.LoadFromDBAsync();
            var existingEntry = entries.FirstOrDefault(e => e.Serv_name == title && e.Serv_email == user);
            if (existingEntry != null)
            {
                throw new Exception("An entry with the same name and username already exists.");
            }

            await ServiceEntry.CreateAsync(title, user, pw, mfa, algorithm);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
            return;
        }

        var mainWindow = Application.Current?.Windows.FirstOrDefault();
        if (mainWindow != null)
        {
            mainWindow.Page = new ListScreen();
        }
    }

    private void OnPasswordToggleClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton toggleButton && toggleButton.CommandParameter is Entry associatedEntry)
        {
            TogglePasswordVisibility(associatedEntry, toggleButton);
        }
    }

    private void TogglePasswordVisibility(Entry passwordEntry, ImageButton toggleButton)
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

    private async void GoToScan(object sender, EventArgs e)
    {
        await OpenQRScanner(async qrCodeValue =>
        {
            if (!string.IsNullOrEmpty(qrCodeValue))
            {
                var (secret, issuer, accountName, algorithm) = ParseOtpAuthUrl(qrCodeValue);

                // Update UI or perform other actions based on scanned data
                await Dispatcher.DispatchAsync(async () =>
                {
                    MFA.Text = secret;
                    AlgorithmPicker.SelectedItem = algorithm;

                    bool saveOver = await DisplayAlert("Save Over?", "Do you want to save the provided Service name and user as well?", "Yes", "No");
                    if (saveOver)
                    {
                        Serv.Text = issuer;
                        User.Text = accountName;
                    }
                });
            }
        });
    }

    private async Task OpenQRScanner(Func<string, Task> onQRCodeScanned)
    {
        var qrScanner = new QRReader
        {
            OnQRCodeScanned = onQRCodeScanned
        };

        // Open QR scanning screen as a modal
        await Navigation.PushModalAsync(qrScanner);
    }

    private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
    {
        SliderValueLabel.Text = ((int)e.NewValue).ToString();
    }

    private async void CopyPass_Clicked(object sender, EventArgs e)
    {
        await Clipboard.Default.SetTextAsync(Pass.Text);
        var toast = Toast.Make("Password copied to clipboard", ToastDuration.Short);
        await toast.Show();
    }

    private (string secret, string issuer, string accountName, string algorithm) ParseOtpAuthUrl(string url)
    {
        var uri = new Uri(url);
        var query = HttpUtility.ParseQueryString(uri.Query);

        string secret = query.Get("secret");
        string issuer = query.Get("issuer");
        string algorithm = query.Get("algorithm") ?? "SHA1"; // Default to SHA1 if not specified
        string accountName = uri.AbsolutePath.Split(':').LastOrDefault()?.TrimStart('/');

        return (secret, issuer, accountName, algorithm);
    }
}

