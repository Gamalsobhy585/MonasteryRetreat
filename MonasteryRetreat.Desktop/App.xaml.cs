using System;
using System.Windows;
using MonasteryRetreat.Infrastructure.Persistence;
using MonasteryRetreat.Infrastructure.Persistence.Seeders;
using MonasteryRetreat.Desktop.Views.Reservations;

namespace MonasteryRetreat.Desktop;

public partial class App : System.Windows.Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Close the app when the main window (reservation window) closes.
        ShutdownMode = ShutdownMode.OnMainWindowClose;

        try
        {
            var factory = new MonasteryDbContextFactory();

            await using var db = factory.CreateDbContext(
                Array.Empty<string>());

            // Apply migrations and seed initial data.
            await DatabaseSeeder.SeedAsync(db);

            // Create reservation window.
            var window = new NewReservationWindow();

            // Load governorates from SQLite.
            await window.LoadGovernoratesAsync(db);

            // Set main application window.
            MainWindow = window;

            // Display reservation window.
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