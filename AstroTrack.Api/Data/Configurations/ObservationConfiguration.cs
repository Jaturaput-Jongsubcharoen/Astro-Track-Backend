using AstroTrack.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AstroTrack.Api.Data.Configurations;

/// <summary>
/// Maps the OBSERVATIONS Oracle table to the Observation entity.
/// </summary>
public class ObservationConfiguration : IEntityTypeConfiguration<Observation>
{
    public void Configure(EntityTypeBuilder<Observation> builder)
    {
        builder.ToTable("OBSERVATIONS");

        builder.HasKey(observation => observation.ObservationId);

        builder.Property(observation => observation.ObservationId)
            .HasColumnName("OBSERVATION_ID")
            .HasColumnType("NUMBER");

        builder.Property(observation => observation.ObjectId)
            .HasColumnName("OBJECT_ID")
            .HasColumnType("NUMBER")
            .IsRequired();

        builder.Property(observation => observation.TelescopeId)
            .HasColumnName("TELESCOPE_ID")
            .HasColumnType("NUMBER")
            .IsRequired();

        builder.Property(observation => observation.ResearcherId)
            .HasColumnName("RESEARCHER_ID")
            .HasColumnType("NUMBER")
            .IsRequired();

        builder.Property(observation => observation.ObservationDate)
            .HasColumnName("OBSERVATION_DATE")
            .IsRequired();

        builder.Property(observation => observation.XrayFlux)
            .HasColumnName("XRAY_FLUX")
            .HasPrecision(10, 3);

        builder.Property(observation => observation.Redshift)
            .HasColumnName("REDSHIFT")
            .HasPrecision(6, 5);
    }
}
