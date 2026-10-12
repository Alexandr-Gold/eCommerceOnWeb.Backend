using eCommerceOnWeb.Backend.Domain.Aggregates.BrandAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eCommerceOnWeb.Backend.Persistence.Contexts.EntityTypeConfigurations;

public sealed class BrandConfiguration
    : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("brands");

        // --- Primary Key ---
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Ignore(b => b.DomainEvents);

        // --- Basic Properties ---
        builder.Property(b => b.Name)
            .HasColumnName("name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(b => b.Slug)
            .HasColumnName("slug")
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(b => b.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(b => b.LogoStorageKey)
            .HasColumnName("logo_storage_key")
            .HasMaxLength(1000);

        builder.Property(b => b.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(b => b.DisplayOrder)
            .HasColumnName("display_order")
            .IsRequired();

        // --- Indexes ---
        builder.HasIndex(b => b.Name)
            .IsUnique();

        builder.HasIndex(b => b.Slug)
            .IsUnique();

        builder.HasIndex(b => b.IsActive);
    }
}