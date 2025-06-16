using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using QRCoder;

namespace LockBox;

public partial class MenuPopup : Popup
{
    public MenuPopup()
    {
        InitializeComponent();
    }

    private async void OnResetClicked(object sender, EventArgs e)
    {
        string action = await Application.Current!.Windows[0].Page!.DisplayActionSheet("Do you want to reset this device? All saved items on this device will be gone", "No", null, "Reset");
        if (action == "Reset")
        {
            await CredMan.DeleteCredentialsAsync();
            string filePath = Path.Combine(FileSystem.AppDataDirectory, "entries.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            if (Application.Current is null) { return; }
            await Application.Current.Windows[0].Navigation.PushAsync(new Setup());
            Close();
        }
    }

    private void OnAboutClicked(object sender, EventArgs e)
    {
        if (Application.Current is null) { return; }
        Application.Current.Windows[0].Page?.DisplayAlert("About", $"Version :{AppInfo.Current.VersionString}", "OK");
        Close();
    }

    private async void OnTransferClicked(object sender, EventArgs e)
    {
        string? user = await CredMan.GetUsernameAsync();
        if (user == null) { return; }
        string? salt = Convert.ToBase64String(await CredMan.GetSaltAsync());

        string text = "";
        text += user + ",";
        text += Convert.ToBase64String(await CredMan.GetPasswordAsync()) + ",";
        text += salt;
        await Application.Current!.Windows[0].Navigation.PushModalAsync(new QrTransfer(GenerateQrCode(text),user ,salt ));
        Close();
    }
    private ImageSource GenerateQrCode(string text)
    {
        using (var qrGenerator = new QRCodeGenerator())
        {
            var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(20);

            return ImageSource.FromStream(() => new MemoryStream(qrCodeBytes));
        }
    }
}
