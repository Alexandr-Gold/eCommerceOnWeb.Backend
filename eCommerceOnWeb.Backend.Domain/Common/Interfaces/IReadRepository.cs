using Ardalis.Specification;

namespace eCommerceOnWeb.Backend.Domain.Common.Interfaces
{
    public interface IReadRepository<T> : IReadRepositoryBase<T> where T : class, IAggregateRoot
    {
    }
}
