using eCommerceOnWeb.Backend.Domain.Common;
using eCommerceOnWeb.Backend.Domain.Exceptions;

namespace eCommerceOnWeb.Backend.Domain.Aggregates.BrandAggregate;
public sealed class Brand
    : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? LogoStorageKey { get; private set; }

    public bool IsActive { get; private set; } = true;

    public int DisplayOrder { get; private set; }


    private Brand()
    {
    }


    private Brand(
        string name,
        string slug)
    {
        Name = name;
        Slug = slug;
    }


    public static Brand Create(
        string name,
        string slug)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(
                "Brand name is required.");

        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainException(
                "Brand slug is required.");

        return new Brand(
            name.Trim(),
            slug.Trim().ToLowerInvariant());
    }


    public void SetLogo(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
            throw new DomainException("Storage key is required.");

        LogoStorageKey = storageKey;
    }


    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
    }
}