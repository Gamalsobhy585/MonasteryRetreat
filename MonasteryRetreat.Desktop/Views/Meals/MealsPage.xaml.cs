using System.Windows;
using System.Windows.Controls;
using MonasteryRetreat.Application.Meals;

namespace MonasteryRetreat.Desktop.Views.Meals;

public partial class MealsPage : UserControl
{
    private readonly IDailyMealMenuService _service;
    private bool _isRefreshing;
    private bool _refreshQueued;
    private IReadOnlyList<DailyMealMenuRow> _menus = Array.Empty<DailyMealMenuRow>();

    public MealsPage(IDailyMealMenuService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        InitializeComponent();
        MenuDatePicker.SelectedDate = DateTime.Today;
        Loaded += async (_, _) =>
        {
            await RefreshAsync();
            if (MenuDatePicker.SelectedDate is DateTime date)
                LoadDate(DateOnly.FromDateTime(date));
        };
    }

    private void LoadDate(DateOnly date)
    {
        var menu = _menus.FirstOrDefault(x => x.MenuDate == date);
        BreakfastBox.Text = menu?.Breakfast ?? string.Empty;
        LunchBox.Text = menu?.Lunch ?? string.Empty;
        DinnerBox.Text = menu?.Dinner ?? string.Empty;
    }

    private async Task RefreshAsync()
    {
        if (_isRefreshing)
        {
            _refreshQueued = true;
            return;
        }

        _isRefreshing = true;
        try
        {
            do
            {
                _refreshQueued = false;
                _menus = await _service.ListAsync();
                MealsGrid.ItemsSource = _menus;
                EmptyText.Visibility = _menus.Count == 0
                    ? Visibility.Visible : Visibility.Collapsed;
            }
            while (_refreshQueued);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"تعذر تحميل الوجبات:\n{ex.Message}",
                "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    private async void MenuDateChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        await RefreshAsync();
        if (MenuDatePicker.SelectedDate is DateTime date)
            LoadDate(DateOnly.FromDateTime(date));
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not DailyMealMenuRow row)
            return;

        MenuDatePicker.SelectedDate = row.MenuDate.ToDateTime(TimeOnly.MinValue);
        LoadDate(row.MenuDate);
        BreakfastBox.Focus();
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (MenuDatePicker.SelectedDate is not DateTime date)
        {
            MessageBox.Show("اختر التاريخ أولًا.", "بيانات ناقصة",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        ContentRoot.IsEnabled = false;
        try
        {
            await _service.SaveAsync(
                DateOnly.FromDateTime(date),
                BreakfastBox.Text,
                LunchBox.Text,
                DinnerBox.Text);
            await RefreshAsync();
            LoadDate(DateOnly.FromDateTime(date));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"تعذر حفظ الوجبات:\n{ex.Message}",
                "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            ContentRoot.IsEnabled = true;
        }
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        await RefreshAsync();
        if (MenuDatePicker.SelectedDate is DateTime date)
            LoadDate(DateOnly.FromDateTime(date));
    }
}
