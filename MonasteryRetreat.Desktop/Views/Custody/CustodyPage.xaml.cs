using System.Windows;
using System.Windows.Controls;
using MonasteryRetreat.Application.Custody;

namespace MonasteryRetreat.Desktop.Views.Custody;

public partial class CustodyPage : UserControl
{
    private readonly ICustodyService _service;
    private bool _isRefreshing;
    private bool _refreshQueued;
    private int _historyPage = 1;
    private bool _historyLoading;

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

                if (ReturnedHistoryExpander.IsExpanded)
                    await LoadReturnedHistoryAsync();
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

    private async Task LoadReturnedHistoryAsync()
    {
        if (_historyLoading) return;
        _historyLoading = true;
        try
        {
            var result = await _service.GetReturnedHistoryAsync(_historyPage);
            _historyPage = result.Page;
            ReturnedGrid.ItemsSource = result.Items;
            ReturnedEmptyText.Visibility = result.TotalCount == 0
                ? Visibility.Visible : Visibility.Collapsed;
            HistoryPageText.Text = $"صفحة {result.Page} من {result.TotalPages} — {result.TotalCount} سجل";
            PreviousHistoryButton.IsEnabled = result.Page > 1;
            NextHistoryButton.IsEnabled = result.Page < result.TotalPages;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"تعذر تحميل سجل المستردات:\n{ex.Message}",
                "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _historyLoading = false;
        }
    }

    private async void ReturnedHistory_Expanded(object sender, RoutedEventArgs e)
        => await LoadReturnedHistoryAsync();

    private void ReturnedHistory_Collapsed(object sender, RoutedEventArgs e)
    {
        // Keep the currently selected page for the next expansion.
    }

    private async void HistoryPrevious_Click(object sender, RoutedEventArgs e)
    {
        if (_historyLoading || _historyPage <= 1) return;
        _historyPage--;
        await LoadReturnedHistoryAsync();
    }

    private async void HistoryNext_Click(object sender, RoutedEventArgs e)
    {
        if (_historyLoading || !NextHistoryButton.IsEnabled) return;
        _historyPage++;
        await LoadReturnedHistoryAsync();
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
