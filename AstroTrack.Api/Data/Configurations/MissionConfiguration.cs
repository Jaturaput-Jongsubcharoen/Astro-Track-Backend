using AstroTrack.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AstroTrack.Api.Data.Configurations;

/// <summary>
/// Maps the MISSIONS Oracle table to the Mission entity.
/// </summary>
public class MissionConfiguration : IEntityTypeConfiguration<Mission>
{
    public void Configure(EntityTypeBuilder<Mission> builder)
    {
        builder.ToTable("MISSIONS");

        builder.HasKey(mission => mission.MissionId);

        builder.Property(mission => mission.MissionId)
            .HasColumnName("MISSION_ID")
            .HasColumnType("NUMBER");

        builder.Property(mission => mission.MissionName)
            .HasColumnName("MISSION_NAME")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(mission => mission.MissionPurpose)
            .HasColumnName("MISSION_PURPOSE")
            .HasMaxLength(100);

        builder.Property(mission => mission.StartDate)
            .HasColumnName("START_DATE")
            .IsRequired();

        builder.Property(mission => mission.EndDate)
            .HasColumnName("END_DATE");

        builder.Property(mission => mission.LeadResearcherId)
            .HasColumnName("LEAD_RESEARCHER_ID")
            .HasColumnType("NUMBER")
            .IsRequired();

        builder.Property(mission => mission.AffiliationId)
            .HasColumnName("AFFILIATION_ID")
            .HasColumnType("NUMBER")
            .IsRequired();
    }
}
