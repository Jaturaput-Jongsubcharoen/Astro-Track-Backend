using AstroTrack.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AstroTrack.Api.Data.Configurations;

/// <summary>
/// Maps the CELESTIALOBJECTS Oracle table to the CelestialObject entity.
/// </summary>
public class CelestialObjectConfiguration : IEntityTypeConfiguration<CelestialObject>
{
    private static readonly ValueConverter<bool, string> YesNoConverter =
        new(
            v => v ? "Y" : "N",
            v => v == "Y" || v == "y");

    public void Configure(EntityTypeBuilder<CelestialObject> builder)
    {
        builder.ToTable("CELESTIALOBJECTS");

        builder.HasKey(e => e.ObjectId);

        builder.Property(e => e.ObjectId)
            .HasColumnName("OBJECT_ID")
            .HasColumnType("NUMBER");

        builder.Property(e => e.ObjectName)
            .HasColumnName("OBJECT_NAME")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.Category)
            .HasColumnName("CATEGORY")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.DistanceLightYears)
            .HasColumnName("DISTANCE_LIGHT_YEARS")
            .HasPrecision(16, 6);

        builder.Property(e => e.DiscoveryDate)
            .HasColumnName("DISCOVERY_DATE");

        builder.Property(e => e.InSolarSystem)
            .HasColumnName("IN_SOLAR_SYSTEM")
            .HasConversion(YesNoConverter)
            .HasColumnType("CHAR(1)");

        builder.Property(e => e.HabitabilityScore)
            .HasColumnName("HABITABILITY_SCORE")
            .HasPrecision(4, 2);

        builder.Property(e => e.SurfaceTemperature)
            .HasColumnName("SURFACE_TEMPERATURE")
            .HasPrecision(12, 2);

        builder.Property(e => e.Gravity)
            .HasColumnName("GRAVITY")
            .HasPrecision(5, 2);

        builder.Property(e => e.Nitrogen)
            .HasColumnName("NITROGEN")
            .HasConversion(YesNoConverter)
            .HasColumnType("CHAR(1)");

        builder.Property(e => e.Oxygen)
            .HasColumnName("OXYGEN")
            .HasConversion(YesNoConverter)
            .HasColumnType("CHAR(1)");

        builder.Property(e => e.Co2)
            .HasColumnName("CO2")
            .HasConversion(YesNoConverter)
            .HasColumnType("CHAR(1)");

        builder.Property(e => e.SulfuricAcid)
            .HasColumnName("SULFURIC_ACID")
            .HasConversion(YesNoConverter)
            .HasColumnType("CHAR(1)");

        builder.Property(e => e.Hydrogen)
            .HasColumnName("HYDROGEN")
            .HasConversion(YesNoConverter)
            .HasColumnType("CHAR(1)");

        builder.Property(e => e.Helium)
            .HasColumnName("HELIUM")
            .HasConversion(YesNoConverter)
            .HasColumnType("CHAR(1)");

        builder.Property(e => e.Methane)
            .HasColumnName("METHANE")
            .HasConversion(YesNoConverter)
            .HasColumnType("CHAR(1)");

        builder.Property(e => e.WaterVapor)
            .HasColumnName("WATER_VAPOR")
            .HasConversion(YesNoConverter)
            .HasColumnType("CHAR(1)");

        builder.Property(e => e.Silicates)
            .HasColumnName("SILICATES")
            .HasConversion(YesNoConverter)
            .HasColumnType("CHAR(1)");

        builder.Property(e => e.Iron)
            .HasColumnName("IRON")
            .HasConversion(YesNoConverter)
            .HasColumnType("CHAR(1)");

        builder.Property(e => e.Nickel)
            .HasColumnName("NICKEL")
            .HasConversion(YesNoConverter)
            .HasColumnType("CHAR(1)");
    }
}
