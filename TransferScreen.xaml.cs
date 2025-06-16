namespace LockBox;

public partial class TransferScreen : ContentPage
{
    private string? scannedPassword;

    public TransferScreen()
    {
        InitializeComponent();
    }

    private async void Restore(object sender, EventArgs e)
    {
        var username = UserId.Text;
        var password = Password.Text;
        var salt = Salt.Text;

        if (!ValidateData(username, password, salt))
        {
            if (scannedPassword != null && !await CredMan.IsPasswordCorrectAsync(scannedPassword, password))
            {
                await DisplayAlert("Error", "The password provided does not match the scanned password.", "OK");
            }
        }

        try
        {
            await CredMan.SaveSaltAsync(Convert.FromBase64String(salt));
            await CredMan.SaveCredentialsAsync(username, password);
            await DisplayAlert("Success", "Credentials restored successfully.", "OK");
            Application.Current!.Windows[0].Page = new ListScreen();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to restore credentials: {ex.Message}", "OK");
        }
    }

    private async void Cancel(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private bool ValidateData(string username, string password, string salt)
    {
        if (string.IsNullOrEmpty(username))
        {
            DisplayAlert("Validation Error", "Username cannot be empty.", "OK");
            return false;
        }

        if (string.IsNullOrEmpty(password))
        {
            DisplayAlert("Validation Error", "Password cannot be empty.", "OK");
            return false;
        }

        if (!CredMan.ValidatePassword(password))
        {
            DisplayAlert("Validation Error", "Password does not meet the required criteria.", "OK");
            return false;
        }

        if (string.IsNullOrEmpty(salt))
        {
            DisplayAlert("Validation Error", "Salt cannot be empty.", "OK");
            return false;
        }

        try
        {
            Convert.FromBase64String(salt);
        }
        catch (FormatException)
        {
            DisplayAlert("Validation Error", "Salt is not in a valid Base64 format.", "OK");
            return false;
        }

        return true;
    }

    private async void ScanQRCode(object sender, EventArgs e)
    {
        await OpenQRScanner(async qrCodeValue =>
        {
            try
            {
                var (username, password, salt) = ParseScannedData(qrCodeValue);
                UserId.Text = username;
                scannedPassword = password;
                Salt.Text = salt;
            }
            catch (FormatException ex)
            {
                await DisplayAlert("Error", $"Failed to parse QR code data: {ex.Message}", "OK");
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

    private (string username, string password, string salt) ParseScannedData(string data)
    {
        var parts = data.Split(',');
        if (parts.Length != 3)
        {
            throw new FormatException("Invalid data format. Check the QR code and try again");
        }

        return (parts[0], parts[1], parts[2]);
    }
}
