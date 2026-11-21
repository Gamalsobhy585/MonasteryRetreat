using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonasteryRetreat.Domain.Entities;

namespace MonasteryRetreat.Infrastructure.Persistence.Configurations;

public sealed class SpeakerAssignmentConfiguration : IEntityTypeConfiguration<SpeakerAssignment>
{
    public void Configure(EntityTypeBuilder<SpeakerAssignment> builder)
    {
        builder.ToTable("speaker_assignments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SpeakingDate).IsRequired();
        builder.Property(x => x.SpeakerName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
        builder.HasIndex(x => x.SpeakingDate).IsUnique();
    }
}
