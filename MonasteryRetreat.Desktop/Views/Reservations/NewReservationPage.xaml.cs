
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using MonasteryRetreat.Infrastructure.Persistence;
using MonasteryRetreat.Application.Guests;
using MonasteryRetreat.Application.Reservations;
namespace MonasteryRetreat.Desktop.Views.Reservations;

public partial class NewReservationPage : UserControl
{
    private readonly ICreateReservationService _createReservationService;
    private readonly IGuestSearchService _guestSearchService;
    private bool _isSaving;
    private bool _isSearching;
    private GuestSummary? _selectedGuest;

    public NewReservationPage(
        ICreateReservationService createReservationService,
        IGuestSearchService guestSearchService)
    {
        _createReservationService =
            createReservationService
            ?? throw new ArgumentNullException(
                nameof(createReservationService));

        _guestSearchService =
            guestSearchService
            ?? throw new ArgumentNullException(
                nameof(guestSearchService));

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

    private async void SearchGuest_Click(
        object sender,
        RoutedEventArgs e)
    {
        await SearchGuestsAsync();
    }

    private async void GuestSearchTextBox_KeyDown(
        object sender,
        System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
        {
            e.Handled = true;
            await SearchGuestsAsync();
        }
    }

    private async Task SearchGuestsAsync()
    {
        if (_isSearching)
            return;

        string term = GuestSearchTextBox.Text.Trim();

        if (term.Length < 2)
        {
            SearchStatusText.Text = "اكتب حرفين على الأقل ثم اضغط بحث.";
            return;
        }

        try
        {
            _isSearching = true;
            SearchStatusText.Text = "جاري البحث...";

            IReadOnlyList<GuestSummary> results =
                await _guestSearchService.SearchAsync(term);

            GuestResultsGrid.ItemsSource = results;
            ClearSelectedGuest();

            SearchStatusText.Text = results.Count == 0
                ? "لا توجد نتائج. تأكد من الاسم أو الرقم، أو اختر \"أول مرة\" لتسجيل ضيف جديد."
                : $"تم العثور على {results.Count} ضيف — اختر الضيف من القائمة.";
        }
        catch (Exception ex)
        {
            SearchStatusText.Text = string.Empty;

            MessageBox.Show(
                $"حدث خطأ أثناء البحث:\n{ex.Message}",
                "خطأ",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            _isSearching = false;
        }
    }

    private void GuestResultsGrid_SelectionChanged(
        object sender,
        System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (GuestResultsGrid.SelectedItem is GuestSummary guest)
        {
            _selectedGuest = guest;

            SelectedGuestText.Text =
                $"الضيف المختار: {guest.FullName}  |  {guest.Phone}";

            SelectedGuestCard.Visibility = Visibility.Visible;
        }
        else
        {
            ClearSelectedGuest();
        }
    }

    private void ClearSelectedGuest()
    {
        _selectedGuest = null;
        SelectedGuestText.Text = string.Empty;
        SelectedGuestCard.Visibility = Visibility.Collapsed;
    }

    private bool TryBuildExistingGuestReservation(
        out ExistingGuestReservationInput? input)
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

        if (_selectedGuest == null)
        {
            ShowError("يرجى البحث عن الضيف واختياره من القائمة");
            GuestSearchTextBox.Focus();
            return false;
        }

        input = new ExistingGuestReservationInput
        {
            GuestId = _selectedGuest.Id,
            ExpectedArrival = arrival.Date,
            ExpectedDeparture = departure.Date,
            Notes = PreviousNotesTextBox.Text.Trim()
        };

        return true;
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

        phone = EgyptianPhone.Normalize(phone);

        if (!EgyptianPhone.IsValid(phone))
        {
            ShowError(EgyptianPhone.ErrorMessage);
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

    private async void SaveReservation_Click(
        object sender,
        RoutedEventArgs e)
    {
        await SubmitReservationAsync();
    }
    private async void SaveAndAdd_Click(
      object sender,
      RoutedEventArgs e)
    {
        await SubmitReservationAsync();
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

        // Previous-guest search state
        GuestSearchTextBox.Clear();
        PreviousNotesTextBox.Clear();
        GuestResultsGrid.ItemsSource = null;
        ClearSelectedGuest();
        SearchStatusText.Text = "اكتب حرفين على الأقل ثم اضغط بحث.";

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

    private async Task SubmitReservationAsync()
    {
        if (_isSaving)
            return;

        bool isPreviousGuest = PreviousGuestRadio.IsChecked == true;

        NewReservationInput? newGuestInput = null;
        ExistingGuestReservationInput? existingGuestInput = null;

        bool valid = isPreviousGuest
            ? TryBuildExistingGuestReservation(out existingGuestInput)
            : TryBuildReservation(out newGuestInput);

        if (!valid)
            return;

        try
        {
            _isSaving = true;

            if (isPreviousGuest)
            {
                await _createReservationService
                    .CreateForExistingGuestAsync(existingGuestInput!);
            }
            else
            {
                await _createReservationService
                    .CreateAsync(newGuestInput!);
            }

            MessageBox.Show(
                "تم حفظ الحجز بنجاح.",
                "نجاح",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // Stay on this screen (it is the main window, so closing it
            // would exit the whole application) and get ready for the
            // next reservation.
            ResetForm();
        }
        catch (ReservationValidationException ex)
        {
            ShowError(ex.Message);
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
            ResetForm();
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
