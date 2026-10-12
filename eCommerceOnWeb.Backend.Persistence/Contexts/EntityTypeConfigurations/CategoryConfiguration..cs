using eCommerceOnWeb.Backend.Domain.Aggregates.CategoryAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eCommerceOnWeb.Backend.Persistence.Contexts.EntityTypeConfigurations;

public sealed class CategoryConfiguration
    : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        // --- Primary Key ---
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Ignore(c => c.DomainEvents);

        // --- Basic Properties ---
        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(c => c.Slug)
            .HasColumnName("slug")
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(c => c.ImageStorageKey)
            .HasColumnName("image_storage_key")
            .HasMaxLength(1000);

        builder.Property(c => c.ParentCategoryId)
            .HasColumnName("parent_category_id");

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(c => c.DisplayOrder)
            .HasColumnName("display_order")
            .IsRequired();

        // --- Indexes ---
        builder.HasIndex(c => c.Slug)
            .IsUnique();

        builder.HasIndex(c => c.ParentCategoryId);

        builder.HasIndex(c => c.IsActive);

        // --- Self-reference ---
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}