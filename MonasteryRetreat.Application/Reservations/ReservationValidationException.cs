namespace MonasteryRetreat.Application.Reservations;

/// <summary>
/// A business-rule violation whose message is safe to show to the user as is.
/// </summary>
public sealed class ReservationValidationException : Exception
{
    public ReservationValidationException(string message)
        : base(message)
    {
    }
}
