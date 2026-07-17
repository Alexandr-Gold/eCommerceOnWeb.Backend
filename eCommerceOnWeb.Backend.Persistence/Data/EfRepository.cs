using Ardalis.Specification.EntityFrameworkCore;
using eCommerceOnWeb.Backend.Domain.Common;
using eCommerceOnWeb.Backend.Domain.Common.Interfaces;

namespace eCommerceOnWeb.Backend.Persistence.Data
{
    public class EfRepository<T> : RepositoryBase<T>, IReadRepository<T>, IRepository<T> where T : class, IAggregateRoot
    {
        // Передаем наш кастомный ApplicationDbContext в базовый класс RepositoryBase из библиотеки Ardalis
        public EfRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
