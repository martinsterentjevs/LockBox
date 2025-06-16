namespace LockBox;

public class QrTransfer : ContentPage
{
    private Image QrCodeImage;
    private Button CloseButton;
    private Label UsernameLabel;
    private Label SaltLabel;

    public QrTransfer(ImageSource qrCodeItem, string username, string salt)
    {
        QrCodeImage = new Image
        {
            Source = qrCodeItem,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            MaximumHeightRequest = 200,
            MaximumWidthRequest = 200
        };

        CloseButton = new Button
        {
            Text = "Close",
            HorizontalOptions = LayoutOptions.Center
        };
        CloseButton.Clicked += CloseButton_Clicked;

        Label Instructions = new Label
        {
            Text = "Scan this code on the device you want this transferred to or use the data down below if you are unable to scan",
            FontSize = 24,
            TextColor = (Color)Application.Current!.Resources["PrimaryDark"],
            HorizontalTextAlignment = TextAlignment.Justify
        };

        UsernameLabel = new Label
        {
            Text = $"Username: {username}",
            FontSize = 16,
            TextColor = (Color)Application.Current.Resources["SecondaryDarkText"],
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Start
        };

        SaltLabel = new Label
        {
            Text = $"Salt: {salt}",
            FontSize = 16,
            TextColor = (Color)Application.Current.Resources["SecondaryDarkText"],
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment= TextAlignment.Start
        };

        var gridContent = new Grid
        {
            VerticalOptions = LayoutOptions.CenterAndExpand,
            HorizontalOptions = LayoutOptions.CenterAndExpand,
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            Children =
            {
                new Label
                {
                    Text = "QR Code",
                    FontSize = 48,
                    HorizontalOptions = LayoutOptions.Center,
                    TextColor = (Color)Application.Current.Resources["PrimaryDark"]
                },
                Instructions,
                QrCodeImage,
                UsernameLabel,
                SaltLabel,
                CloseButton
            }
        };

        Grid.SetRow(Instructions, 1);
        Grid.SetRow(QrCodeImage, 2);
        Grid.SetRow(UsernameLabel, 3);
        Grid.SetRow(SaltLabel, 4);
        Grid.SetRow(CloseButton, 5);

        Content = new Grid
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            BackgroundColor = (Color)Application.Current.Resources["OffBlack"],
            Children = { gridContent }
        };

        Title = "TransferPopup";
    }

    private async void CloseButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
