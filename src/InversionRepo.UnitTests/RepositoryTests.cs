using InversionRepo.Interfaces;
using InversionRepo.UnitTests.Dtos;
using InversionRepo.UnitTests.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace InversionRepo.UnitTests
{
    public class RepositoryTests
    {
        Repository<SalesAppContext> _repo;
        public RepositoryTests()
        {
            var options = new DbContextOptionsBuilder<SalesAppContext>()
                   .UseInMemoryDatabase(databaseName: "InversionTests")
                   .EnableSensitiveDataLogging()
                   .Options;

            var context = new SalesAppContext(options);

            // in a real use case for a web api, Repository<SalesAppContext> would be injected as a scoped service for IRepository<SalesAppContext>, and
            // context would be injected automatically if you use services.AddDbContext inside the ConfigureServices on Startup.cs
            _repo = new Repository<SalesAppContext>(context);
        }

        
        [Fact]
        public async void ProjectedList()
        {
            // this is still being resolved by https://github.com/dotnet/efcore/issues/17620
            // the error only happens with an in-memory database. With SqlServer is all good
            var orders = await _repo.ProjectedList(OrderDto.ProjectionFromEntity(), o => Order.HasItemsOverPrice(o, 90));//entity => entity.Items.Any(i => i.Price > 90)); // o => Order.HasItemsOverPrice(o, 90));
            Assert.Equal(54, orders.Count);

            Assert.Equal(50, orders[0].Items[0].Quantity);
            Assert.Equal(30, orders[0].Items[1].Quantity);

            Assert.Equal(80, orders[0].Items[0].Price);
            Assert.Equal(120, orders[0].Items[1].Price);
        }

        [Theory]
        [InlineData(10, 0, null, true)]
        [InlineData(10, 1, null, true)]
        [InlineData(10, 2, null, true)]
        [InlineData(10, 3, null, true)]
        [InlineData(10, 4, null, true)]
        [InlineData(10, 5, null, true)]
        public async Task ProjectedListBuilder(int? pageSize, int? pageNumber, string sortField, bool sortOrderAscending)
        {
            var listRequest = new ListRequest { PageSize = pageSize, PageNumber = pageNumber, SortField = sortField, SortOrderAscending = sortOrderAscending };

            var builder = _repo.ProjectedListBuilder(OrderDto.ProjectionFromEntity(), listRequest)
                .OrderBy(o => o.Id)
                .ConditionalOrder("customerName", o => o.Customer.Name)
                .ConditionalOrder("fullAddress", o => o.DeliveryAddress.FullAdress);

            var orders = await builder.ExecuteAsync();
            var totalCount = await builder.CountAsync();

            var expectedCount = 55;
            Assert.Equal(expectedCount, totalCount);

            if (pageNumber <= 4)
            {
                Assert.Equal(pageSize, orders.Count);
            }
            else
            {
                Assert.Equal(5, orders.Count); // last page has 5 records
            }

            if (sortField == null)
            {
                var orderId = (pageNumber * pageSize) + 1;
                Assert.Equal($"Address for OrderId {orderId}, country: Brazil", orders[0].FullDeliveryAdressWithCountry);
                Assert.Equal(orderId, orders[0].Id);
            }
        }

    }

    class ListRequest : IListRequest
    {
        public int? PageSize { get; set; }
        public int? PageNumber { get; set; }
        public string SortField { get; set; }
        public bool SortOrderAscending { get; set; }
    }
}
