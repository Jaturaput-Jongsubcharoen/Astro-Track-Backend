using AstroTrack.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AstroTrack.Api.Data.Configurations;

/// <summary>
/// Maps the RESEARCHERS Oracle table to the Researcher entity.
/// </summary>
public class ResearcherConfiguration : IEntityTypeConfiguration<Researcher>
{
    public void Configure(EntityTypeBuilder<Researcher> builder)
    {
        builder.ToTable("RESEARCHERS");

        builder.HasKey(researcher => researcher.ResearcherId);

        builder.Property(researcher => researcher.ResearcherId)
            .HasColumnName("RESEARCHER_ID")
            .HasColumnType("NUMBER");

        builder.Property(researcher => researcher.ResearcherName)
            .HasColumnName("RESEARCHER_NAME")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(researcher => researcher.ContactEmail)
            .HasColumnName("CONTACT_EMAIL")
            .HasMaxLength(50);

        builder.Property(researcher => researcher.PhoneNumber)
            .HasColumnName("PHONE_NUMBER")
            .HasMaxLength(15);

        builder.Property(researcher => researcher.AffiliationId)
            .HasColumnName("AFFILIATION_ID")
            .HasColumnType("NUMBER")
            .IsRequired();
    }
}