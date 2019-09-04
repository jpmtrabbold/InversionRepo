using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using InversionRepo.UnitTests.Entities;

namespace InversionRepo.UnitTests.Dtos
{
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductQuantityInStock { get; set; }
        public List<ProductDiscountDto> Discounts { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }

        public static Expression<Func<OrderItem, OrderItemDto>> ProjectionFromEntity()
        {
            return entity => new OrderItemDto()
            {
                Id = entity.Id,
                ProductId = entity.ProductId,
                ProductName = entity.Product.Name,
                ProductQuantityInStock = entity.Product.QuantityInStock,
                Quantity = entity.Quantity,
                Price = entity.Price,
                Discounts = entity.Product.Discounts.AsQueryable().Select(ProductDiscountDto.ProjectionFromEntity()).ToList(),
            };
        }
    }
}