using System.Collections.Generic;

namespace InversionRepo.UnitTests.Entities
{
    public partial class Order : BaseEntity
    {
        public int? CustomerId { get; set; }
        public Customer Customer { get; set; }
        public int DeliveryAddressId { get; set; }
        public Address DeliveryAddress { get; set; }
        public ICollection<OrderItem> Items { get; private set; } = new HashSet<OrderItem>();
    }
}