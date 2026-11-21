using System.Windows;
using System.Windows.Controls;
using MonasteryRetreat.Application.Custody;

namespace MonasteryRetreat.Desktop.Views.Custody;

public partial class CustodyPage : UserControl
{
    private readonly ICustodyService _service;
    private bool _isRefreshing;
    private bool _refreshQueued;

    public CustodyPage(ICustodyService service)
    {
        _service = service;
        InitializeComponent();
        Loaded += async (_, _) => await RefreshAsync();
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
                var selectedStayId = (GuestSelector.SelectedItem as CustodyGuestOption)?.StayId;
                var overview = await _service.GetOverviewAsync();

                GuestSelector.ItemsSource = overview.Guests;
                GuestSelector.SelectedItem = overview.Guests
                    .FirstOrDefault(g => g.StayId == selectedStayId);

                ItemsGrid.ItemsSource = overview.Items;
                OutstandingCountText.Text = overview.OutstandingCount.ToString();
                EmptyText.Visibility = overview.Items.Count == 0
                    ? Visibility.Visible : Visibility.Collapsed;
            } while (_refreshQueued);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"تعذر تحميل المتعلقات:\n{ex.Message}",
                "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
        => await RefreshAsync();

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        if (GuestSelector.SelectedItem is not CustodyGuestOption guest)
        {
            MessageBox.Show("اختر الضيف أولًا.", "بيانات ناقصة",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(QuantityBox.Text, out var quantity))
        {
            MessageBox.Show("أدخل كمية صحيحة.", "بيانات ناقصة",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        ContentRoot.IsEnabled = false;
        try
        {
            await _service.AddAsync(
                guest.StayId, DescriptionBox.Text, quantity,
                StorageBox.Text, NotesBox.Text);
            DescriptionBox.Clear();
            QuantityBox.Text = "1";
            StorageBox.Clear();
            NotesBox.Clear();
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"تعذر تسجيل المتعلقات:\n{ex.Message}",
                "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            ContentRoot.IsEnabled = true;
        }
    }

    private async void Return_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not CustodyItemRow item ||
            !item.IsOutstanding)
            return;

        var confirm = MessageBox.Show(
            $"تأكيد تسليم «{item.ItemDescription}» إلى {item.GuestName}؟",
            "تسليم المتعلقات", MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        ContentRoot.IsEnabled = false;
        try
        {
            await _service.ReturnAsync(item.Id);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"تعذر تسليم المتعلقات:\n{ex.Message}",
                "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            ContentRoot.IsEnabled = true;
        }
    }
}
