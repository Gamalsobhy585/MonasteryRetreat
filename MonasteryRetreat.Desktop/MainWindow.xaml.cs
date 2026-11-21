using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MonasteryRetreat.Desktop.Views.Reservations;

namespace MonasteryRetreat.Desktop;

/// <summary>
/// The application shell: a sidebar and the page currently shown.
/// Closing this window closes the application.
/// </summary>
public partial class MainWindow : Window
{
    private static readonly Brush ActiveBrush =
        new SolidColorBrush(Color.FromRgb(0xAA, 0x79, 0x53));

    private static readonly Brush InactiveBrush =
        new SolidColorBrush(Color.FromRgb(0x6B, 0x45, 0x2F));

    private readonly NewReservationPage _newReservationPage;
    private readonly ReservationsPage _reservationsPage;

    public MainWindow(
        NewReservationPage newReservationPage,
        ReservationsPage reservationsPage)
    {
        _newReservationPage = newReservationPage;
        _reservationsPage = reservationsPage;

        InitializeComponent();

        // Today's reservations are the first thing staff need to see.
        ShowReservations();
    }

    private void ShowReservations()
    {
        PageHost.Content = _reservationsPage;
        SetActive(ReservationsNavButton);
    }

    private void ShowNewReservation()
    {
        PageHost.Content = _newReservationPage;
        SetActive(NewReservationNavButton);
    }

    private void SetActive(Button active)
    {
        ReservationsNavButton.Background =
            ReferenceEquals(active, ReservationsNavButton)
                ? ActiveBrush
                : InactiveBrush;

        NewReservationNavButton.Background =
            ReferenceEquals(active, NewReservationNavButton)
                ? ActiveBrush
                : InactiveBrush;
    }

    private void ReservationsNav_Click(object sender, RoutedEventArgs e)
        => ShowReservations();

    private void NewReservationNav_Click(object sender, RoutedEventArgs e)
        => ShowNewReservation();
}
