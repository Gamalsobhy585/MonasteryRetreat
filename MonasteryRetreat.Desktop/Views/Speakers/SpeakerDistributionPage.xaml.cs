using System.Windows;
using System.Windows.Controls;
using MonasteryRetreat.Application.Speakers;

namespace MonasteryRetreat.Desktop.Views.Speakers;

public partial class SpeakerDistributionPage : UserControl
{
    private readonly ISpeakerAssignmentService _service;
    private bool _loading;
    private bool _refreshQueued;

    public SpeakerDistributionPage(ISpeakerAssignmentService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        InitializeComponent();
        SpeakingDatePicker.SelectedDate = DateTime.Today;
        Loaded += async (_, _) => await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        if (_loading)
        {
            _refreshQueued = true;
            return;
        }

        _loading = true;
        try
        {
            do
            {
                _refreshQueued = false;
                var rows = await _service.ListAsync();
                SpeakersGrid.ItemsSource = rows;
                EmptyText.Visibility = rows.Count == 0
                    ? Visibility.Visible : Visibility.Collapsed;
            }
            while (_refreshQueued);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"تعذر تحميل توزيع المتكلمين:\n{ex.Message}",
                "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _loading = false;
        }
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not SpeakerAssignmentRow row)
            return;

        SpeakingDatePicker.SelectedDate = row.SpeakingDate.ToDateTime(TimeOnly.MinValue);
        SpeakerNameTextBox.Text = row.SpeakerName;
        SpeakerNameTextBox.Focus();
        SpeakerNameTextBox.SelectAll();
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (SpeakingDatePicker.SelectedDate is not DateTime date)
        {
            MessageBox.Show("اختر التاريخ أولًا.", "بيانات ناقصة",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        ContentRoot.IsEnabled = false;
        try
        {
            await _service.SaveAsync(DateOnly.FromDateTime(date), SpeakerNameTextBox.Text);
            await RefreshAsync();
            SpeakerNameTextBox.Clear();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"تعذر حفظ المتكلم:\n{ex.Message}",
                "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            ContentRoot.IsEnabled = true;
        }
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
        => await RefreshAsync();
}
