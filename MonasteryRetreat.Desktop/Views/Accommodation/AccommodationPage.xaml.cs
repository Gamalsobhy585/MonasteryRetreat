using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MonasteryRetreat.Application.Accommodation;
using MonasteryRetreat.Application.Reservations;

namespace MonasteryRetreat.Desktop.Views.Accommodation;

public partial class AccommodationPage : UserControl
{
    private readonly IAccommodationService _service;

    private bool _isRefreshing;
    private bool _refreshQueued;

    public AccommodationPage(IAccommodationService service)
    {
        _service = service
            ?? throw new ArgumentNullException(nameof(service));

        InitializeComponent();

        CurrentDateText.Text = DateTime.Now.ToString(
            "dddd، dd MMMM yyyy",
            new CultureInfo("ar-EG"));

        // Fires every time the page is shown, so it is always up to date
        // (for example after a stay was ended on the reservations page).
        Loaded += async (_, _) => await RefreshAsync();
    }

    public async Task RefreshAsync()
    {
        // The services share one database context, so never run two
        // loads at once: queue one more pass instead.
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

                AccommodationOverview overview =
                    await _service.GetOverviewAsync();

                RoomsItemsControl.ItemsSource = overview.Rooms;
                UnassignedGrid.ItemsSource = overview.Unassigned;
                AssignedGrid.ItemsSource = overview.Assigned;

                UnassignedEmptyText.Visibility =
                    overview.Unassigned.Count == 0
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                AssignedEmptyText.Visibility =
                    overview.Assigned.Count == 0
                        ? Visibility.Visible
                        : Visibility.Collapsed;

            } while (_refreshQueued);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"حدث خطأ أثناء تحميل بيانات التسكين:\n{ex.Message}",
                "خطأ",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    private async Task RunActionAsync(Func<Task> action)
    {
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

        // Reload either way: rooms may have filled up in the meantime.
        await RefreshAsync();
    }

    private async void Assign_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext
            is not UnassignedGuestRow row)
        {
            return;
        }

        if (row.SelectedRoom == null)
        {
            MessageBox.Show(
                "يرجى اختيار الغرفة أولًا.",
                "التسكين",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        await RunActionAsync(
            () => _service.AssignAsync(row.StayId, row.SelectedRoom.Id));
    }

    private async void Move_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext
            is not AssignedGuestRow row)
        {
            return;
        }

        if (row.SelectedRoom == null)
        {
            MessageBox.Show(
                "يرجى اختيار الغرفة المنقول إليها أولًا.",
                "النقل",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        await RunActionAsync(
            () => _service.MoveAsync(row.StayId, row.SelectedRoom.Id));
    }
}
