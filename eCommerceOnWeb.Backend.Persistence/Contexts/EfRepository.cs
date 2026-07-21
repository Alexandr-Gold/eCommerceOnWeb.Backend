using Ardalis.Specification.EntityFrameworkCore;
using eCommerceOnWeb.Backend.Domain.Common;
using eCommerceOnWeb.Backend.Domain.Common.Interfaces;

namespace eCommerceOnWeb.Backend.Persistence.Contexts
{
    public class EfRepository<T> : RepositoryBase<T>, IReadRepository<T>, IRepository<T> where T : class, IAggregateRoot
    {
        // Передаем наш кастомный ECommerceDbContext в базовый класс RepositoryBase из библиотеки Ardalis
        public EfRepository(ECommerceDbContext dbContext) : base(dbContext)
        {
        }
    }
}
