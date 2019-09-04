using InversionRepo.UnitTests.Entities;
using InversionRepo.UnitTests.Extensions;
using Microsoft.EntityFrameworkCore;

namespace InversionRepo.UnitTests
{
    public class DataSeeding
    {
        internal static void Seed(ModelBuilder mb)
        {
            mb.HasData(new Product { Id = 1, Name = "Cereal", QuantityInStock = 10 });
            mb.HasData(new Product { Id = 2, Name = "Milk", QuantityInStock = 100 });
            var ps4Id = mb.HasData(new Product { Id = 3, Name = "PS4", QuantityInStock = 40 });
            var ponyId = mb.HasData(new Product { Id = 4, Name = "Pony", QuantityInStock = 3 });

            mb.HasData(new ProductDiscount { Id = 1, ProductId = ps4Id, MinimumQuantityPurchasedBefore = 2, DiscountFactor = 0.01M  });
            mb.HasData(new ProductDiscount { Id = 2, ProductId = ps4Id, MinimumQuantityPurchasedBefore = 5, DiscountFactor = 0.10M });
            mb.HasData(new ProductDiscount { Id = 3, ProductId = ps4Id, MinimumQuantityPurchasedBefore = 20, DiscountFactor = 0.20M });
            mb.HasData(new ProductDiscount { Id = 4, ProductId = ponyId, MinimumQuantityPurchasedBefore = 2, DiscountFactor = 0.25M });

            var addressId = mb.HasData(new Address { Id = 1, FullAdress = "12 Masters St, Onewamasaka", Country = "Peru" });
            mb.HasData(new Customer { Id = 1, Name = "Alfred Mukutata", AddressId = addressId });
            addressId = mb.HasData(new Address { Id = 2, FullAdress = "34 Ronaldo St, Favela da Goiaba", Country = "Paquistan" });
            mb.HasData(new Customer { Id = 2, Name = "Ronnie Sanders", AddressId = addressId });
            addressId = mb.HasData(new Address { Id = 3, FullAdress = "334 Pacific St, California", Country = "Uruguay" });
            mb.HasData(new Customer { Id = 3, Name = "Creusa Soares", AddressId = addressId });

            var customerId = 0;
            var productId = 0;
            var productId2 = 2;

            for (int i = 1; i <= 55; i++)
            {
                customerId++;
                if (customerId > 3) customerId = 1;

                productId++;
                if (productId > 4) productId = 1;

                productId2++;
                if (productId2 > 4) productId2 = 1;

                var id = i * 1000;

                addressId = mb.HasData(new Address { Id = id, FullAdress = $"Address for OrderId {i}", Country = "Brazil" });
                mb.HasData(new Order { Id = i, CustomerId = customerId, DeliveryAddressId = addressId });
                mb.HasData(new OrderItem { Id = id, OrderId = i, ProductId = productId, Quantity = i * 25, Price = i * 40, });
                mb.HasData(new OrderItem { Id = id+1, OrderId = i, ProductId = productId2, Quantity = i * 15, Price = i * 60, });
            }
        }
    }
}