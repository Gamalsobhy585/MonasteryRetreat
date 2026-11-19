
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonasteryRetreat.Domain.Entities;

namespace MonasteryRetreat.Infrastructure.Persistence.Configurations;

public class RoomAssignmentConfiguration
    : IEntityTypeConfiguration<RoomAssignment>
{
    public void Configure(EntityTypeBuilder<RoomAssignment> builder)
    {
        builder.ToTable("room_assignments");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.StayId)
            .IsUnique()
            .HasFilter("released_at IS NULL");

        builder.HasIndex(x => x.RoomId);

        builder.HasOne(x => x.Stay)
            .WithMany(x => x.RoomAssignments)
            .HasForeignKey(x => x.StayId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Room)
            .WithMany(x => x.RoomAssignments)
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

      
    }
}
