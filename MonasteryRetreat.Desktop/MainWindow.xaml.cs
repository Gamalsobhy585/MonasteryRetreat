using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MonasteryRetreat.Desktop.Views.Accommodation;
using MonasteryRetreat.Desktop.Views.Reservations;
using MonasteryRetreat.Desktop.Views.Liturgy;
using MonasteryRetreat.Desktop.Views.Custody;

namespace MonasteryRetreat.Desktop;

/// <summary>
/// The application shell: a sidebar and the page currently shown.
/// Closing this window closes the application.
/// </summary>
public partial class MainWindow : Window
{
    private static readonly Brush ActiveBrush =
        (Brush)System.Windows.Application.Current.Resources["GoldBrush"];

    private static readonly Brush InactiveBrush =
        (Brush)System.Windows.Application.Current.Resources["SidebarNavBrush"];

    private readonly NewReservationPage _newReservationPage;
    private readonly ReservationsPage _reservationsPage;
    private readonly AccommodationPage _accommodationPage;
    private readonly LiturgyAssignmentsPage _liturgyPage;
    private readonly CustodyPage _custodyPage;

    public MainWindow(
        NewReservationPage newReservationPage,
        ReservationsPage reservationsPage,
        AccommodationPage accommodationPage,
        LiturgyAssignmentsPage liturgyPage,
        CustodyPage custodyPage)
    {
        _newReservationPage = newReservationPage;
        _reservationsPage = reservationsPage;
        _accommodationPage = accommodationPage;
        _liturgyPage = liturgyPage;
        _custodyPage = custodyPage;

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

    private void ShowCustody()
    {
        PageHost.Content = _custodyPage;
        SetActive(CustodyNavButton);
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

        CustodyNavButton.Background =
            ReferenceEquals(active, CustodyNavButton)
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

    private void CustodyNav_Click(object sender, RoutedEventArgs e)
        => ShowCustody();

    private void NewReservationNav_Click(object sender, RoutedEventArgs e)
        => ShowNewReservation();
}
