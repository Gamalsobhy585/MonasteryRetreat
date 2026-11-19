
using Microsoft.EntityFrameworkCore;
using MonasteryRetreat.Domain.Entities;

namespace MonasteryRetreat.Infrastructure.Persistence;

public class MonasteryDbContext : DbContext
{
    public MonasteryDbContext(
        DbContextOptions<MonasteryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<Governorate> Governorates => Set<Governorate>();

    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Stay> Stays => Set<Stay>();
    public DbSet<RoomAssignment> RoomAssignments => Set<RoomAssignment>();
    public DbSet<StayAdjustment> StayAdjustments => Set<StayAdjustment>();

    public DbSet<CustodyItem> CustodyItems => Set<CustodyItem>();

    public DbSet<MonasteryChurch> MonasteryChurches
        => Set<MonasteryChurch>();

    public DbSet<LiturgySchedule> LiturgySchedules
        => Set<LiturgySchedule>();

    public DbSet<LiturgyAssignment> LiturgyAssignments
        => Set<LiturgyAssignment>();

    public DbSet<CheckoutRecord> CheckoutRecords
        => Set<CheckoutRecord>();

  



    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    public DbSet<RetreatProgramItem> RetreatProgramItems
        => Set<RetreatProgramItem>();

    public DbSet<RetreatInstruction> RetreatInstructions
        => Set<RetreatInstruction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(MonasteryDbContext).Assembly);
    }
}
