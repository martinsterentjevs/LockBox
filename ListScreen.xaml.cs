using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace LockBox;

public partial class ListScreen : ContentPage
{
    private IDispatcherTimer? _initialTimer;
    private IDispatcherTimer? _intervalTimer;
    private ListScreenViewModel _viewModel;

    public ListScreen()
    {
        InitializeComponent();
        _viewModel = new ListScreenViewModel();
        BindingContext = _viewModel; // Set the BindingContext to the view model
        LoadServiceEntriesAsync();
    }

    private async void LoadServiceEntriesAsync()
    {
        Debug.WriteLine("Loading service entries...");
        var entries = await ServiceEntry.LoadFromDBAsync();

        // Clear the existing items
        _viewModel.ListItems.Clear();

        // Add the loaded entries to the view model
        foreach (var entry in entries)
        {
            var decryptedEntry = await entry.GetDecrypted();
            var viewModelItem = new ServiceEntryViewModel
            {
                Db_id = entry.Db_id,
                Serv_name = decryptedEntry.Serv_name,
                Serv_email = decryptedEntry.Serv_email,
                Serv_password = decryptedEntry.Serv_password,
                Serv_mfasec = decryptedEntry.Serv_mfasec,
                Algorithm = decryptedEntry.Algorithm,
                OtpCode = ServiceEntry.GetCurrentCode(decryptedEntry)
            };
            _viewModel.ListItems.Add(viewModelItem);
        }

        // Initialize the initial timer
        _initialTimer = Dispatcher.CreateTimer();
        _initialTimer.Interval = CalculateInitialDelay();
        _initialTimer.Tick += OnInitialTimerTick;
        _initialTimer.Start();

        Debug.WriteLine($"Initial timer set to trigger in {_initialTimer.Interval.TotalMilliseconds} milliseconds.");
    }

    private TimeSpan CalculateInitialDelay()
    {
        DateTime now = DateTime.UtcNow;
        int seconds = now.Second + 1;
        int milliseconds = now.Millisecond;

        // Calculate the delay to the next 30-second mark
        int delaySeconds = (seconds < 30) ? (30 - seconds) : (60 - seconds);
        int delayMilliseconds = 1000 - milliseconds;

        return new TimeSpan(0, 0, 0, delaySeconds, delayMilliseconds);
    }

    private void OnInitialTimerTick(object? sender, EventArgs e)
    {
        // Stop the initial timer
        _initialTimer?.Stop();

        // Update the OTP codes
        _viewModel.UpdateOtpCodes();

        // Initialize the interval timer
        _intervalTimer = Dispatcher.CreateTimer();
        _intervalTimer.Interval = TimeSpan.FromSeconds(30);
        _intervalTimer.Tick += OnIntervalTimerTick;
        _intervalTimer.Start();

        Debug.WriteLine($"Interval timer set to trigger every {_intervalTimer.Interval.TotalSeconds} seconds.");
    }

    private void OnIntervalTimerTick(object? sender, EventArgs e)
    {
        // Update the OTP codes
        _viewModel.UpdateOtpCodes();

        Debug.WriteLine($"Interval timer triggered at {DateTime.UtcNow:HH:mm:ss.fff}");
    }

    private async void RefreshList()
    {
        Debug.WriteLine("Refreshing list...");
        var entries = await ServiceEntry.LoadFromDBAsync();

        // Clear the existing items
        _viewModel.ListItems.Clear();

        // Add the loaded entries to the view model
        foreach (var entry in entries)
        {
            var decryptedEntry = await entry.GetDecrypted();
            var viewModelItem = new ServiceEntryViewModel
            {
                Db_id = entry.Db_id,
                Serv_name = decryptedEntry.Serv_name,
                Serv_email = decryptedEntry.Serv_email,
                Serv_password = decryptedEntry.Serv_password,
                Serv_mfasec = decryptedEntry.Serv_mfasec,
                Algorithm = decryptedEntry.Algorithm,
                OtpCode = ServiceEntry.GetCurrentCode(decryptedEntry)
            };
            _viewModel.ListItems.Add(viewModelItem);
        }
    }

    private void Menu_Clicked(object sender, EventArgs e)
    {
        var popup = new MenuPopup();
        this.ShowPopup(popup);
    }

    private async void RefreshList(object sender, EventArgs e)
    {
        Debug.WriteLine("Refreshing list...");
        var entries = await ServiceEntry.LoadFromDBAsync();

        // Clear the existing items
        _viewModel.ListItems.Clear();

        // Add the loaded entries to the view model
        foreach (var entry in entries)
        {
            var decryptedEntry = await entry.GetDecrypted();
            var viewModelItem = new ServiceEntryViewModel
            {
                Db_id = entry.Db_id,
                Serv_name = decryptedEntry.Serv_name,
                Serv_email = decryptedEntry.Serv_email,
                Serv_password = decryptedEntry.Serv_password,
                Serv_mfasec = decryptedEntry.Serv_mfasec,
                Algorithm = decryptedEntry.Algorithm,
                OtpCode = ServiceEntry.GetCurrentCode(decryptedEntry)
            };
            _viewModel.ListItems.Add(viewModelItem);
        }
    }

    private async void AddEntry(object sender, EventArgs e)
    {
        if (Application.Current is null)
        {
            return;
        }
        Debug.WriteLine("Adding new entry...");
        await Application.Current.Windows[0].Navigation.PushModalAsync(new AddScreen());
    }

    private void OnCopyOptionsClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.CommandParameter is ServiceEntryViewModel viewModel)
        {
            var popup = new CopyOptionsPopup(viewModel);
            this.ShowPopup(popup);
        }
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        var button = sender as ImageButton;
        var dbId = button?.CommandParameter as int?;

        if (dbId.HasValue)
        {
            var editScreen = new EditScreen(dbId.Value);
            await Navigation.PushModalAsync(editScreen);
        }
    }
}
public partial class ListScreenViewModel : INotifyPropertyChanged
{
    public ObservableCollection<ServiceEntryViewModel> ListItems { get; set; }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public ListScreenViewModel()
    {
        ListItems = ServiceEntry.ListItems;
        Debug.WriteLine("Loaded in " + ServiceEntry.ListItems.Count + " Items to CollectionView");
        PropertyChanged = delegate { }; // Initialize the PropertyChanged event with an empty delegate
    }

    public void UpdateOtpCodes()
    {
        Debug.WriteLine("UpdateOtpCodes called");
        foreach (var item in ListItems)
        {
            item.UpdateOtpCode();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
