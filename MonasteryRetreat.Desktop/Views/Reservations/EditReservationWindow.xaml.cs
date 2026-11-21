using System;
using System.Windows;
using MonasteryRetreat.Application.Reservations;

namespace MonasteryRetreat.Desktop.Views.Reservations;

/// <summary>
/// Edits the dates and notes of one reservation. Saves by itself and only
/// closes (with DialogResult = true) when the save succeeded, so a rejected
/// edit does not lose what the user typed.
/// </summary>
public partial class EditReservationWindow : Window
{
    private readonly IReservationManagementService _service;
    private readonly ReservationListItem _item;

    public EditReservationWindow(
        IReservationManagementService service,
        ReservationListItem item)
    {
        _service = service
            ?? throw new ArgumentNullException(nameof(service));

        _item = item
            ?? throw new ArgumentNullException(nameof(item));

        InitializeComponent();

        GuestNameText.Text = item.GuestName;

        ArrivalDatePicker.SelectedDate =
            item.ExpectedArrival.ToDateTime(TimeOnly.MinValue);

        DepartureDatePicker.SelectedDate =
            item.ExpectedDeparture.ToDateTime(TimeOnly.MinValue);

        NotesTextBox.Text = item.Notes ?? string.Empty;

        if (item.State == ReservationState.InHouse)
        {
            // The guest has already arrived: only the departure moves.
            ArrivalDatePicker.IsEnabled = false;

            HintText.Text =
                "الضيف مقيم حاليًا، لذلك يمكن تعديل تاريخ المغادرة والملاحظات فقط.";
        }
        else
        {
            HintText.Text = "عدّل موعد الحجز أو الملاحظات.";
        }
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (ArrivalDatePicker.SelectedDate is not DateTime arrival ||
            DepartureDatePicker.SelectedDate is not DateTime departure)
        {
            MessageBox.Show(
                "يرجى تحديد تاريخ الوصول والمغادرة",
                "التحقق من البيانات",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            SaveButton.IsEnabled = false;

            await _service.UpdateAsync(
                _item.ReservationId,
                arrival.Date,
                departure.Date,
                NotesTextBox.Text);

            DialogResult = true;
        }
        catch (ReservationValidationException ex)
        {
            MessageBox.Show(
                ex.Message,
                "التحقق من البيانات",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"حدث خطأ أثناء حفظ التعديل:\n{ex.Message}",
                "خطأ",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            SaveButton.IsEnabled = true;
        }
    }
}
