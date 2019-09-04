using InversionRepo.UnitTests.Entities;
using LinqExpander;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace InversionRepo.UnitTests.Dtos
{
    public class OrderDto
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string FullDeliveryAdressWithCountry { get; set; }
        public List<OrderItemDto> Items { get; private set; } = new List<OrderItemDto>();
        public bool HasItemsWithPriceOver100 { get; set; }

        internal static Expression<Func<Order, OrderDto>> ProjectionFromEntity()
        {
            return entity => new OrderDto()
            {
                Id = entity.Id,
                CustomerId = entity.CustomerId,
                CustomerName = entity.Customer.Name,
                FullDeliveryAdressWithCountry = entity.DeliveryAddress.FullAdress + ", country: " + entity.DeliveryAddress.Country,
                Items = entity.Items.AsQueryable().Select(OrderItemDto.ProjectionFromEntity()).ToList(),
                HasItemsWithPriceOver100 = Order.HasItemsOverPrice(entity, 100),
            };
        }
    }
}