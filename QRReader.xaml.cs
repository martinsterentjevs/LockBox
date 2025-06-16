using System.Web;
using ZXing.Net.Maui;

namespace LockBox;

public partial class QRReader : ContentPage
{
    public QRReader()
    {
        InitializeComponent();
        OnQRCodeScanned = async (qrCodeValue) => await Task.CompletedTask; // Initialize with a default delegate
    }
    private bool _isScanning = true;
    public Func<string, Task> OnQRCodeScanned { get; set; } // Callback for handling QR code data  

    private void OnBarcodeDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_isScanning)
        {
            _isScanning = false; // Stop scanning after the first result  
            var qrCodeValue = e.Results.FirstOrDefault()?.Value;

            if (!string.IsNullOrEmpty(qrCodeValue))
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    // Invoke the callback and close the page  
                    if (OnQRCodeScanned != null)
                        await OnQRCodeScanned(qrCodeValue);

                    await Navigation.PopModalAsync();
                });
            }
        }
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync(); // Close the scanner without scanning  
    }
}
