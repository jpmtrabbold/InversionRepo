using System;
using System.Linq.Expressions;
using InversionRepo.UnitTests.Entities;

namespace InversionRepo.UnitTests.Dtos
{
    public class ProductDiscountDto
    {
        public int Id { get; set; }
        public decimal MinimumQuantityPurchasedBefore { get; set; }
        public decimal DiscountFactor { get; set; }

        internal static Expression<Func<ProductDiscount, ProductDiscountDto>> ProjectionFromEntity()
        {
            return entity => new ProductDiscountDto()
            {
                Id = entity.Id,
                MinimumQuantityPurchasedBefore = entity.MinimumQuantityPurchasedBefore,
                DiscountFactor = entity.DiscountFactor,
            };
        }
    }
}