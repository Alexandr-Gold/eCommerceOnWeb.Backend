//using eCommerceOnWeb.Backend.Domain.Common;
//using eCommerceOnWeb.Backend.Domain.Exceptions;

//namespace eCommerceOnWeb.Backend.Domain.Aggregates.CategoryAggregate
//{
//    public class Category : BaseEntity, IAggregateRoot, ISoftDeletable
//    {
//        public string Name { get; private set; } = string.Empty;
//        public string Slug { get; private set; } = string.Empty; // URL-friendly имя (например, "smartphones")
//        public Guid? ParentCategoryId { get; private set; }       // Для древовидной структуры

//        // Поля мягкого удаления
//        public bool IsDeleted { get; private set; }
//        public DateTime? DeletedAtUtc { get; private set; }

//        private Category() { } // Для EF Core

//        public Category(string name, string slug, Guid? parentCategoryId = null)
//        {
//            Update(name, slug, parentCategoryId);
//        }

//        public void Update(string name, string slug, Guid? parentCategoryId)
//        {
//            if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Название категории не может быть пустым.");
//            if (string.IsNullOrWhiteSpace(slug)) throw new DomainException("Slug категории не может быть пустым.");

//            // Защита от зацикливания: категория не может быть родителем самой себя
//            if (parentCategoryId == Id && Id != Guid.Empty)
//                throw new DomainException("Категория не может быть родительской для самой себя.");

//            Name = name.Trim();
//            Slug = slug.Trim().ToLower();
//            ParentCategoryId = parentCategoryId;
//        }

//        public void Delete()
//        {
//            if (IsDeleted) return;
//            IsDeleted = true;
//            DeletedAtUtc = DateTime.UtcNow;
//        }
//    }
//}
