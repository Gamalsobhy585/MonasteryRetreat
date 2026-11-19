
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using MonasteryRetreat.Infrastructure.Persistence;
using MonasteryRetreat.Application.Reservations;
namespace MonasteryRetreat.Desktop.Views.Reservations;

public partial class NewReservationWindow : Window
{
    private readonly ICreateReservationService _createReservationService;
    private bool _isSaving;

    public NewReservationWindow(
        ICreateReservationService createReservationService)
    {
        _createReservationService =
            createReservationService
            ?? throw new ArgumentNullException(
                nameof(createReservationService));

        InitializeComponent();
        InitializeDefaults();
    }

    private void InitializeDefaults()
    {
        CurrentDateText.Text = DateTime.Now.ToString(
            "dddd، dd MMMM yyyy",
            new CultureInfo("ar-EG"));

        ArrivalDatePicker.SelectedDate =
            DateTime.Today.AddDays(2);

        DepartureDatePicker.SelectedDate =
            DateTime.Today.AddDays(4);
    }

    private void ArrivalDateChanged(
        object sender,
        System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (DepartureDatePicker == null)
            return;

        if (ArrivalDatePicker.SelectedDate is not DateTime arrival)
            return;

        if (DepartureDatePicker.SelectedDate is not DateTime departure
            || departure <= arrival)
        {
            DepartureDatePicker.SelectedDate = arrival.AddDays(2);
        }
    }

    private void GuestTypeChanged(
        object sender,
        RoutedEventArgs e)
    {
        if (NewGuestSection == null ||
            PreviousGuestSection == null ||
            FirstTimeRadio == null)
        {
            return;
        }

        bool isFirstTime = FirstTimeRadio.IsChecked == true;

        NewGuestSection.Visibility = isFirstTime
            ? Visibility.Visible
            : Visibility.Collapsed;

        PreviousGuestSection.Visibility = isFirstTime
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    public async Task LoadGovernoratesAsync(
        MonasteryDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        List<GovernorateOption> governorates =
            await context.Governorates
                .AsNoTracking()
                .Where(g => g.IsActive)
                .OrderBy(g => g.Id)
                .Select(g => new GovernorateOption
                {
                    Id = g.Id,
                    Name = g.NameAr
                })
                .ToListAsync();

        GovernorateComboBox.ItemsSource = null;

        GovernorateComboBox.DisplayMemberPath =
            nameof(GovernorateOption.Name);

        GovernorateComboBox.SelectedValuePath =
            nameof(GovernorateOption.Id);

        GovernorateComboBox.ItemsSource = governorates;
        GovernorateComboBox.SelectedIndex = -1;
    }

    private bool TryBuildReservation(
        out NewReservationInput? input)
    {
        input = null;

        if (ArrivalDatePicker.SelectedDate
                is not DateTime arrival ||
            DepartureDatePicker.SelectedDate
                is not DateTime departure)
        {
            ShowError("يرجى تحديد تاريخ الوصول والمغادرة");
            return false;
        }

        if (departure <= arrival)
        {
            ShowError("تاريخ المغادرة يجب أن يكون بعد تاريخ الوصول");
            return false;
        }

        if (PreviousGuestRadio.IsChecked == true)
        {
            ShowError("شاشة الضيف السابق لم تُنفذ بعد");
            return false;
        }

        string fullName = FullNameTextBox.Text.Trim();
        string phone = PhoneTextBox.Text.Trim();
        string church = GuestChurchTextBox.Text.Trim();
        string notes = NotesTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            ShowError("يرجى إدخال الاسم الكامل");
            FullNameTextBox.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(phone))
        {
            ShowError("يرجى إدخال رقم الهاتف");
            PhoneTextBox.Focus();
            return false;
        }

        if (GovernorateComboBox.SelectedItem
            is not GovernorateOption governorate)
        {
            ShowError("يرجى اختيار المحافظة");
            GovernorateComboBox.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(church))
        {
            ShowError("يرجى إدخال كنيسة الانتماء");
            GuestChurchTextBox.Focus();
            return false;
        }

        if (!int.TryParse(
                AgeTextBox.Text.Trim(),
                out int age) ||
            age < 1 ||
            age > 120)
        {
            ShowError("يرجى إدخال سن صحيح من 1 إلى 120");
            AgeTextBox.Focus();
            return false;
        }

        input = new NewReservationInput
        {
            ExpectedArrival = arrival.Date,
            ExpectedDeparture = departure.Date,
            FullName = fullName,
            Phone = phone,
            GovernorateId = governorate.Id,
            GuestChurch = church,
            Age = age,
            Notes = notes
        };

        return true;
    }

    private async void SaveButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        await SubmitReservationAsync(false);
    }
    private async void SaveAndAddAnother_Click(
      object sender,
      RoutedEventArgs e)
    {
        await SubmitReservationAsync(true);
    }
    private void ResetForm()
    {
        // Guest information
        FullNameTextBox.Clear();
        PhoneTextBox.Clear();
        GuestChurchTextBox.Clear();
        AgeTextBox.Clear();
        NotesTextBox.Clear();

        // Governorate
        GovernorateComboBox.SelectedIndex = -1;

        // Guest type
        FirstTimeRadio.IsChecked = true;

        // Dates
        ArrivalDatePicker.SelectedDate =
            DateTime.Today.AddDays(2);

        DepartureDatePicker.SelectedDate =
            DateTime.Today.AddDays(4);

        // Focus
        FullNameTextBox.Focus();
    }

    private async Task SubmitReservationAsync(bool addAnother)
    {
        if (_isSaving)
            return;

        if (!TryBuildReservation(out var input))
            return;

        try
        {
            _isSaving = true;

            await _createReservationService.CreateAsync(input);

            MessageBox.Show(
                "تم حفظ الحجز بنجاح.",
                "نجاح",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            if (addAnother)
            {
                ResetForm();
            }
            else
            {
                DialogResult = true;
                Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"حدث خطأ أثناء حفظ الحجز:\n{ex.Message}",
                "خطأ",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            _isSaving = false;
        }
    }

    private void Cancel_Click(
        object sender,
        RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
            "هل تريد إلغاء الحجز الحالي؟",
            "تأكيد الإلغاء",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            Close();
        }
    }

    private static void ShowError(string message)
    {
        MessageBox.Show(
            message,
            "التحقق من البيانات",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }
}

public sealed class GovernorateOption
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;
}

public sealed class NewReservationInput
{
    public DateTime ExpectedArrival { get; set; }

    public DateTime ExpectedDeparture { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public long GovernorateId { get; set; }

    public string GuestChurch { get; set; } = string.Empty;

    public int Age { get; set; }

    public string Notes { get; set; } = string.Empty;
}
