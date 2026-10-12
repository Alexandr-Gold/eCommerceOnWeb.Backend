using eCommerceOnWeb.Backend.Domain.Common;
using eCommerceOnWeb.Backend.Domain.Exceptions;

namespace eCommerceOnWeb.Backend.Domain.Aggregates.CategoryAggregate;
public sealed class Category
    : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? ImageStorageKey { get; private set; }

    public Guid? ParentCategoryId { get; private set; }

    public bool IsActive { get; private set; } = true;

    public int DisplayOrder { get; private set; }


    private Category()
    {
    }


    private Category(
        string name,
        string slug)
    {
        Name = name;
        Slug = slug;
    }


    public static Category Create(
        string name,
        string slug)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(
                "Category name is required.");

        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainException(
                "Category slug is required.");

        return new Category(
            name.Trim(),
            slug.Trim().ToLowerInvariant());
    }


    public void SetImage(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
            throw new DomainException("Storage key is required.");

        ImageStorageKey = storageKey;
    }


    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
    }
}