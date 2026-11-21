using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MonasteryRetreat.Desktop.Views.Accommodation;
using MonasteryRetreat.Desktop.Views.Reservations;
using MonasteryRetreat.Desktop.Views.Liturgy;

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
    private readonly LiturgyAssignmentsPage _liturgyPage;

    public MainWindow(
        NewReservationPage newReservationPage,
        ReservationsPage reservationsPage,
        AccommodationPage accommodationPage,
        LiturgyAssignmentsPage liturgyPage)
    {
        _newReservationPage = newReservationPage;
        _reservationsPage = reservationsPage;
        _accommodationPage = accommodationPage;
        _liturgyPage = liturgyPage;

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

    private void ShowLiturgy()
    {
        PageHost.Content = _liturgyPage;
        SetActive(LiturgyNavButton);
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

        LiturgyNavButton.Background =
            ReferenceEquals(active, LiturgyNavButton)
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

    private void LiturgyNav_Click(object sender, RoutedEventArgs e)
        => ShowLiturgy();

    private void NewReservationNav_Click(object sender, RoutedEventArgs e)
        => ShowNewReservation();
}
