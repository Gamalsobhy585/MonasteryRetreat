using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using MonasteryRetreat.Application.Liturgy;

namespace MonasteryRetreat.Desktop.Views.Liturgy;

public partial class LiturgyAssignmentsPage : UserControl
{
    private readonly ILiturgyAssignmentService _service;
    private bool _loading;
    private bool _refreshPending;

    public List<ChurchOption> ChurchOptions { get; private set; } = new();
    public IReadOnlyList<LiturgyRoleOption> RoleOptions { get; } =
        LiturgyRoles.Names.Select(pair =>
            new LiturgyRoleOption(pair.Key, pair.Value)).ToList();

    public LiturgyAssignmentsPage(ILiturgyAssignmentService service)
    {
        _service = service;
        InitializeComponent();
        DataContext = this;
        DistributionDatePicker.SelectedDate = DateTime.Today;
        Loaded += async (_, _) => await RefreshAsync();
    }

    private DateOnly SelectedDate =>
        DateOnly.FromDateTime(DistributionDatePicker.SelectedDate ?? DateTime.Today);

    private async Task RefreshAsync()
    {
        if (_loading)
        {
            _refreshPending = true;
            return;
        }

        _loading = true;
        try
        {
            do
            {
                _refreshPending = false;
                var overview = await _service.GetOverviewAsync(SelectedDate);
                ChurchOptions = overview.Churches;
                ChurchesItemsControl.ItemsSource = overview.ChurchCards;
                GuestsGrid.ItemsSource = overview.Guests;
                EmptyGuestsText.Visibility = overview.Guests.Count == 0
                    ? Visibility.Visible : Visibility.Collapsed;
                // Refresh the ComboBox ItemsSource binding after loading churches.
                GuestsGrid.Items.Refresh();
                DataContext = null;
                DataContext = this;
            } while (_refreshPending);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"تعذر تحميل توزيع القداسات:\n{ex.Message}",
                "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _loading = false;
        }
    }

    private async void DistributionDateChanged(
        object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded) await RefreshAsync();
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
        => await RefreshAsync();

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not LiturgyGuestRow row)
            return;

        if (row.SelectedChurchId is not long churchId ||
            string.IsNullOrWhiteSpace(row.SelectedRole))
        {
            MessageBox.Show("يرجى اختيار الكنيسة ودور الشماس أولًا.",
                "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        ContentRoot.IsEnabled = false;
        try
        {
            await _service.SaveAsync(SelectedDate, row.StayId,
                churchId, row.SelectedRole);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"تعذر حفظ التوزيع:\n{ex.Message}",
                "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            ContentRoot.IsEnabled = true;
        }
    }
}
