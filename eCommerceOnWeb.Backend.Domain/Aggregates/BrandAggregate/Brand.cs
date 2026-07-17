//using eCommerceOnWeb.Backend.Domain.Common;
//using eCommerceOnWeb.Backend.Domain.Exceptions;

//namespace eCommerceOnWeb.Backend.Domain.Aggregates.BrandAggregate
//{
//    public class Brand : BaseEntity, IAggregateRoot, ISoftDeletable
//    {
//        public string Name { get; private set; } = string.Empty;
//        public string Description { get; private set; } = string.Empty;

//        public bool IsDeleted { get; private set; }
//        public DateTime? DeletedAtUtc { get; private set; }

//        private Brand() { } // Для EF Core

//        public Brand(string name, string description = "")
//        {
//            Update(name, description);
//        }

//        public void Update(string name, string description)
//        {
//            if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Название бренда не может быть пустым.");

//            Name = name.Trim();
//            Description = description.Trim();
//        }

//        public void Delete()
//        {
//            if (IsDeleted) return;
//            IsDeleted = true;
//            DeletedAtUtc = DateTime.UtcNow;
//        }
//    }
//}
