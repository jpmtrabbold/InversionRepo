using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LinqKit;

namespace InversionRepo.UnitTests.Entities
{
    public partial class Order
    {
        public static Expression<Func<Order, bool>> HasItemsOverPrice(decimal price) =>
            (entity) => entity.Items.Any(i => i.Price > price);
    }
}