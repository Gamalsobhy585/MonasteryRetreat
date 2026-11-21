using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MonasteryRetreat.Desktop.Views.Accommodation;
using MonasteryRetreat.Desktop.Views.Reservations;

namespace MonasteryRetreat.Desktop;

/// <summary>
/// The application shell: a sidebar and the page currently shown.
/// Closing this window closes the application.
/// </summary>
public partial class MainWindow : Window
{
    private static readonly Brush ActiveBrush =
        (Brush)Application.Current.Resources["GoldBrush"];

    private static readonly Brush InactiveBrush =
        (Brush)Application.Current.Resources["SidebarNavBrush"];

    private readonly NewReservationPage _newReservationPage;
    private readonly ReservationsPage _reservationsPage;
    private readonly AccommodationPage _accommodationPage;

    public MainWindow(
        NewReservationPage newReservationPage,
        ReservationsPage reservationsPage,
        AccommodationPage accommodationPage)
    {
        _newReservationPage = newReservationPage;
        _reservationsPage = reservationsPage;
        _accommodationPage = accommodationPage;

        InitializeComponent();

        // Today's reservations are the first thing staff need to see.
        ShowReservations();
    }

    private void ShowReservations()
    {
        PageHost.Content = _reservationsPage;
        SetActive(ReservationsNavButton);
    }

    private void ShowAccommodation()
    {
        PageHost.Content = _accommodationPage;
        SetActive(AccommodationNavButton);
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

        AccommodationNavButton.Background =
            ReferenceEquals(active, AccommodationNavButton)
                ? ActiveBrush
                : InactiveBrush;

        NewReservationNavButton.Background =
            ReferenceEquals(active, NewReservationNavButton)
                ? ActiveBrush
                : InactiveBrush;
    }

    private void ReservationsNav_Click(object sender, RoutedEventArgs e)
        => ShowReservations();

    private void AccommodationNav_Click(object sender, RoutedEventArgs e)
        => ShowAccommodation();

    private void NewReservationNav_Click(object sender, RoutedEventArgs e)
        => ShowNewReservation();
}
