using LinqExpander;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

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