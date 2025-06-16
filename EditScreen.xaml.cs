using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using System.Diagnostics;
using System.Web;

namespace LockBox;

public partial class EditScreen : ContentPage
{
    private int _dbId;
    private string _mfa;

    public EditScreen(int dbId)
    {
        try
        {

            InitializeComponent();
            _dbId = dbId;
            LoadDataAsync(dbId);
            _mfa = "";
            Debug.WriteLine("Edit Screen Loaded for: "+dbId);
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void LoadDataAsync(int dbId)
    {
        var entry = ServiceEntry.entries.FirstOrDefault(e => e.Db_id == dbId);
        if (entry != null)
        {
            var decryptedEntry = await entry.GetDecrypted();
            Serv.Text = decryptedEntry.Serv_name;
            User.Text = decryptedEntry.Serv_email;
            Pass.Text = decryptedEntry.Serv_password;
            MFA.Text = decryptedEntry.Serv_mfasec;
            AlgorithmPicker.SelectedItem = decryptedEntry.Algorithm;
        }
    }
    private void GeneratePassword_Clicked(object sender, EventArgs e)
    {
        int passwordLength = (int)PasswordLengthSlider.Value;
        Pass.Text = PasswordGenerator.GeneratePassword(passwordLength);
    }
    private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
    {
        SliderValueLabel.Text = ((int)e.NewValue).ToString();
    }
    public void Back_Clicked(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
    }

    public async void Save_Clicked(object sender, EventArgs e)
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
                throw new Exception("Please fill in all fields");
            }

            // Check for existing entries with the same name and username
            var existingEntry = ServiceEntry.entries.FirstOrDefault(e => e.Serv_name == title && e.Serv_email == user && e.Db_id != _dbId);
            if (existingEntry != null)
            {
                throw new Exception("An entry with the same name and username already exists.");
            }

            // Update the existing entry
            await ServiceEntry.UpdateAsync(_dbId, title, user, pw, mfa, algorithm);

            // Display a success toast notification
            var toast = Toast.Make("Entry updated successfully!", ToastDuration.Short);
            await toast.Show();

            // Send a message to refresh the list 
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(this, "RefreshList");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }

        await Navigation.PopModalAsync();
    }

    private async void Delete_Clicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet("Delete entry?", "No", "Yes");
        if (action == "Yes")
        {
            try
            {
                await ServiceEntry.DeleteAsync(_dbId);
                var toast = Toast.Make("Entry Deleted", ToastDuration.Short);
                await toast.Show();

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
            await Navigation.PopModalAsync();
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

                    bool saveOver = await DisplayAlert("Save Over?", "Do you want to save over the title and email as well?", "Yes", "No");
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
