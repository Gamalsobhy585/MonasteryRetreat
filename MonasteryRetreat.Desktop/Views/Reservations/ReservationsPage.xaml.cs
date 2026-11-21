using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MonasteryRetreat.Application.Reservations;

namespace MonasteryRetreat.Desktop.Views.Reservations;

public partial class ReservationsPage : UserControl
{
    private readonly IReservationManagementService _service;

    // Stops the filter controls from refreshing the list while the page
    // is still being built.
    private bool _suppressRefresh = true;

    private bool _isRefreshing;
    private bool _refreshQueued;

    public ReservationsPage(IReservationManagementService service)
    {
        _service = service
            ?? throw new ArgumentNullException(nameof(service));

        InitializeComponent();

        CurrentDateText.Text = DateTime.Now.ToString(
            "dddd، dd MMMM yyyy",
            new CultureInfo("ar-EG"));

        // Fires every time the page is shown, so the list is always fresh.
        Loaded += async (_, _) =>
        {
            _suppressRefresh = false;
            await RefreshAsync();
        };
    }

    // ---------------------------------------------------------------
    // Filters
    // ---------------------------------------------------------------

    private ReservationQuery BuildQuery()
    {
        var period = ReservationPeriod.Present;

        if (PeriodComboBox.SelectedItem is ComboBoxItem item &&
            Enum.TryParse(item.Tag as string, out ReservationPeriod parsed))
        {
            period = parsed;
        }

        return new ReservationQuery
        {
            Period = period,
            From = FromDatePicker.SelectedDate is DateTime from
                ? DateOnly.FromDateTime(from)
                : null,
            To = ToDatePicker.SelectedDate is DateTime to
                ? DateOnly.FromDateTime(to)
                : null,
            Search = SearchTextBox.Text
        };
    }

    private async void PeriodComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (_suppressRefresh)
            return;

        await RefreshAsync();
    }

    private async void DateFilter_Changed(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (_suppressRefresh)
            return;

        // A date range is meant to search across all periods, so the
        // default "present" period would hide what the user asked for.
        if (PeriodComboBox.SelectedIndex != 3)
        {
            _suppressRefresh = true;
            PeriodComboBox.SelectedIndex = 3;
            _suppressRefresh = false;
        }

        await RefreshAsync();
    }

    private async void SearchTextBox_KeyDown(
        object sender,
        KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            await RefreshAsync();
        }
    }

    private async void Search_Click(object sender, RoutedEventArgs e)
    {
        await RefreshAsync();
    }

    private async void ClearFilters_Click(
        object sender,
        RoutedEventArgs e)
    {
        _suppressRefresh = true;

        PeriodComboBox.SelectedIndex = 0;
        FromDatePicker.SelectedDate = null;
        ToDatePicker.SelectedDate = null;
        SearchTextBox.Clear();

        _suppressRefresh = false;

        await RefreshAsync();
    }

    // ---------------------------------------------------------------
    // Loading
    // ---------------------------------------------------------------

    public async Task RefreshAsync()
    {
        // The services share one database context, so never run two
        // loads at the same time: queue one more pass instead.
        if (_isRefreshing)
        {
            _refreshQueued = true;
            return;
        }

        try
        {
            _isRefreshing = true;

            do
            {
                _refreshQueued = false;

                ReservationListResult result =
                    await _service.ListAsync(BuildQuery());

                ReservationsGrid.ItemsSource = result.Items;

                InHouseCountText.Text =
                    result.InHouseCount.ToString(CultureInfo.InvariantCulture);

                PendingTodayCountText.Text =
                    result.PendingTodayCount.ToString(
                        CultureInfo.InvariantCulture);

                ResultCountText.Text = $"عدد النتائج: {result.Items.Count}";

                EmptyText.Visibility = result.Items.Count == 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            } while (_refreshQueued);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"حدث خطأ أثناء تحميل الحجوزات:\n{ex.Message}",
                "خطأ",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    // ---------------------------------------------------------------
    // Row actions
    // ---------------------------------------------------------------

    private static ReservationListItem? ItemOf(object sender) =>
        (sender as FrameworkElement)?.DataContext as ReservationListItem;

    private bool Confirm(string message) =>
        MessageBox.Show(
            message,
            "تأكيد",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question) == MessageBoxResult.Yes;

    private async Task RunActionAsync(Func<Task> action)
    {
        // Block the page while the change is saved.
        ContentRoot.IsEnabled = false;

        try
        {
            await action();
        }
        catch (ReservationValidationException ex)
        {
            MessageBox.Show(
                ex.Message,
                "لا يمكن تنفيذ العملية",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"حدث خطأ غير متوقع:\n{ex.Message}",
                "خطأ",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            ContentRoot.IsEnabled = true;
        }

        // Reload either way: the state may have changed.
        await RefreshAsync();
    }

    private async void ConfirmArrival_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (ItemOf(sender) is not { } item)
            return;

        if (!Confirm($"تأكيد وصول الضيف \"{item.GuestName}\"؟"))
            return;

        await RunActionAsync(
            () => _service.ConfirmArrivalAsync(item.ReservationId));
    }

    private async void EndStay_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (ItemOf(sender) is not { } item)
            return;

        if (!Confirm($"إنهاء إقامة الضيف \"{item.GuestName}\"؟"))
            return;

        await RunActionAsync(
            () => _service.EndStayAsync(item.ReservationId));
    }

    private async void CancelReservation_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (ItemOf(sender) is not { } item)
            return;

        if (!Confirm($"إلغاء حجز الضيف \"{item.GuestName}\"؟"))
            return;

        await RunActionAsync(
            () => _service.CancelAsync(item.ReservationId));
    }

    private async void Edit_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (ItemOf(sender) is not { } item)
            return;

        var dialog = new EditReservationWindow(_service, item)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true)
            await RefreshAsync();
    }
}
