namespace InversionRepo.UnitTests.Entities
{
    public class ProductDiscount : BaseEntity
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public decimal MinimumQuantityPurchasedBefore { get; set; }
        public decimal DiscountFactor { get; set; }
    }
}