using CommunityToolkit.Maui.Views;
using System.Globalization;

namespace LockBox;

public partial class CopyOptionsPopup : Popup
{
    private readonly ServiceEntryViewModel _viewModel;

    public CopyOptionsPopup(ServiceEntryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private void OnCopyEmailClicked(object sender, EventArgs e)
    {
        Clipboard.SetTextAsync(_viewModel.Serv_email);
        Close();
    }

    private void OnCopyOtpCodeClicked(object sender, EventArgs e)
    {
        Clipboard.SetTextAsync(_viewModel.OtpCode);
        Close();
    }

    private void OnCopyPasswordClicked(object sender, EventArgs e)
    {
        Clipboard.SetTextAsync(_viewModel.Serv_password);
        Close();
    }
}
public class StringToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !string.IsNullOrEmpty(value as string);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class OtpToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !string.IsNullOrEmpty(value as string) && value as string != "MFA Not Set";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
