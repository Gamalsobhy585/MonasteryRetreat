namespace MonasteryRetreat.Application.Accommodation;

/// <summary>A room that can be picked in a drop-down.</summary>
public sealed class RoomOption
{
    public long Id { get; set; }

    /// <summary>Saint name (or "غرفة N") with the current occupancy.</summary>
    public string DisplayName { get; set; } = string.Empty;

    public override string ToString() => DisplayName;
}

/// <summary>One room in the diagram.</summary>
public sealed class RoomOccupancy
{
    public long RoomId { get; set; }

    /// <summary>The saint's name, or "غرفة N" when the room has none.</summary>
    public string Name { get; set; } = string.Empty;

    public string RoomLabel { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public IReadOnlyList<string> Occupants { get; set; }
        = Array.Empty<string>();

    public int Count => Occupants.Count;
    public bool IsFull => Count >= Capacity;
    public bool IsEmpty => Count == 0;
    public string CountText => $"{Count} / {Capacity}";
}

/// <summary>A confirmed guest who is in the house but has no room yet.</summary>
public sealed class UnassignedGuestRow
{
    public long StayId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ArrivalText { get; set; } = string.Empty;
    public string DepartureText { get; set; } = string.Empty;

    /// <summary>Only rooms that still have a free bed.</summary>
    public IReadOnlyList<RoomOption> AvailableRooms { get; set; }
        = Array.Empty<RoomOption>();

    /// <summary>The room picked in the row's drop-down.</summary>
    public RoomOption? SelectedRoom { get; set; }
}

/// <summary>A guest who already has a room and can be moved.</summary>
public sealed class AssignedGuestRow
{
    public long StayId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string CurrentRoomName { get; set; } = string.Empty;

    /// <summary>Rooms with a free bed, except the guest's current room.</summary>
    public IReadOnlyList<RoomOption> AvailableRooms { get; set; }
        = Array.Empty<RoomOption>();

    public RoomOption? SelectedRoom { get; set; }
}

public sealed class AccommodationOverview
{
    public IReadOnlyList<RoomOccupancy> Rooms { get; set; }
        = Array.Empty<RoomOccupancy>();

    public IReadOnlyList<UnassignedGuestRow> Unassigned { get; set; }
        = Array.Empty<UnassignedGuestRow>();

    public IReadOnlyList<AssignedGuestRow> Assigned { get; set; }
        = Array.Empty<AssignedGuestRow>();
}
