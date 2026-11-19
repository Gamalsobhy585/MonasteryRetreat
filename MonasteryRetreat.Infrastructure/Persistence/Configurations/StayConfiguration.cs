
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonasteryRetreat.Domain.Entities;

namespace MonasteryRetreat.Infrastructure.Persistence.Configurations;

public class StayConfiguration : IEntityTypeConfiguration<Stay>
{
    public void Configure(EntityTypeBuilder<Stay> builder)
    {
        builder.ToTable("stays");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.NationalIdAtCheckin)
            .IsRequired()
            .HasMaxLength(14);

        builder.HasIndex(x => x.GuestId)
            .IsUnique()
            .HasFilter("status = 'active'");

        builder.HasIndex(x => x.ReservationId)
            .IsUnique();

        builder.HasOne(x => x.Guest)
            .WithMany(x => x.Stays)
            .HasForeignKey(x => x.GuestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Reservation)
            .WithOne(x => x.Stay)
            .HasForeignKey<Stay>(x => x.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);

        
    }
}
