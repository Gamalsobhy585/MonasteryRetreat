namespace MonasteryRetreat.Application.Reservations;

public enum ReservationPeriod
{
    /// <summary>In the house now, or expected today and not yet arrived.</summary>
    Present,

    /// <summary>Not cancelled, not arrived, arrival date after today.</summary>
    Upcoming,

    /// <summary>Ended stays and no-shows (cancelled ones are not included).</summary>
    Past,

    /// <summary>Cancelled reservations only.</summary>
    Cancelled,

    /// <summary>Everything, cancelled included.</summary>
    All
}

public enum ReservationState
{
    /// <summary>Booked, guest has not arrived yet.</summary>
    Pending,

    /// <summary>Guest arrived and is staying (active stay).</summary>
    InHouse,

    /// <summary>Guest has left (stay ended).</summary>
    Ended,

    Cancelled
}

public sealed class ReservationQuery
{
    public ReservationPeriod Period { get; set; } = ReservationPeriod.Present;

    /// <summary>Only reservations that overlap this range (inclusive).</summary>
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }

    /// <summary>Part of the guest name or phone number.</summary>
    public string? Search { get; set; }
}

public sealed class ReservationListItem
{
    public long ReservationId { get; set; }
    public long GuestId { get; set; }

    public string GuestName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public DateOnly ExpectedArrival { get; set; }
    public DateOnly ExpectedDeparture { get; set; }

    public ReservationState State { get; set; }

    public string? Notes { get; set; }

    public string ArrivalText => ExpectedArrival.ToString("dd/MM/yyyy");
    public string DepartureText => ExpectedDeparture.ToString("dd/MM/yyyy");

    public int Nights =>
        Math.Max(0, ExpectedDeparture.DayNumber - ExpectedArrival.DayNumber);

    public string StateText => State switch
    {
        ReservationState.Pending => "في الانتظار",
        ReservationState.InHouse => "مقيم حاليًا",
        ReservationState.Ended => "انتهت الإقامة",
        ReservationState.Cancelled => "ملغي",
        _ => string.Empty
    };

    // The row buttons. "Today" is captured when the row is built.
    public bool CanConfirm { get; set; }
    public bool CanCancel => State == ReservationState.Pending;
    public bool CanEnd => State == ReservationState.InHouse;
    public bool CanEdit =>
        State == ReservationState.Pending ||
        State == ReservationState.InHouse;
}

public sealed class ReservationListResult
{
    public IReadOnlyList<ReservationListItem> Items { get; set; }
        = Array.Empty<ReservationListItem>();

    /// <summary>Guests currently staying (confirmed, not yet left).</summary>
    public int InHouseCount { get; set; }

    /// <summary>
    /// Reservations for today that the guest has not been confirmed for.
    /// </summary>
    public int PendingTodayCount { get; set; }
}
