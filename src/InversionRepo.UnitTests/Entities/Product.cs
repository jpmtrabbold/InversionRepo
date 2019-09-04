using System.Collections.Generic;

namespace InversionRepo.UnitTests.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; }
        public decimal QuantityInStock { get; set; }
        public ICollection<ProductDiscount> Discounts { get; private set; } = new HashSet<ProductDiscount>();

    }
}