using System;
using System.Windows;
using MonasteryRetreat.Infrastructure.Persistence;
using MonasteryRetreat.Infrastructure.Services;
using MonasteryRetreat.Infrastructure.Persistence.Seeders;
using MonasteryRetreat.Desktop.Views.Accommodation;
using MonasteryRetreat.Desktop.Views.Reservations;
using MonasteryRetreat.Desktop.Views.Liturgy;
using MonasteryRetreat.Desktop.Views.Custody;
using MonasteryRetreat.Desktop.Views.Speakers;
using MonasteryRetreat.Desktop.Views.Meals;

namespace MonasteryRetreat.Desktop;

public partial class App : System.Windows.Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Close the app when the main window (the shell) closes.
        ShutdownMode = ShutdownMode.OnMainWindowClose;

        try
        {
            var factory = new MonasteryDbContextFactory();

            // The context must outlive this method: the window uses it
            // (through the service) until it is closed.
            var db = factory.CreateDbContext(
                Array.Empty<string>());

            // Apply migrations and seed initial data.
            await DatabaseSeeder.SeedAsync(db);

            // Pages share one database context: only one page is
            // visible at a time and every call is awaited in turn.
            var newReservationPage = new NewReservationPage(
                new CreateReservationService(db),
                new GuestSearchService(db));

            var reservationsPage = new ReservationsPage(
                new ReservationManagementService(db));

            var accommodationPage = new AccommodationPage(
                new AccommodationService(db));

            var liturgyPage = new LiturgyAssignmentsPage(
                new LiturgyAssignmentService(db));

            var custodyPage = new CustodyPage(
                new CustodyService(db));

            var speakersPage = new SpeakerDistributionPage(
                new SpeakerAssignmentService(db));

            var mealsPage = new MealsPage(
                new DailyMealMenuService(db));

            // Load governorates from SQLite.
            await newReservationPage.LoadGovernoratesAsync(db);

            var window = new MainWindow(
                newReservationPage,
                reservationsPage,
                accommodationPage,
                liturgyPage,
                custodyPage,
                speakersPage,
                mealsPage);

            window.Closed += (_, _) => db.Dispose();

            // Set main application window.
            MainWindow = window;

            window.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Database initialization failed:\n{ex}",
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown();
        }
    }
}