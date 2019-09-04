using LinqExpander;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace InversionRepo.UnitTests.Entities
{
    public partial class Order
    {
        /// <summary>
        /// this combination between HasItemsOverPrice and HasItemsOverPriceProjection 
        /// enable us to use this expression inside EF expressions to select data
        /// and EF will interpret this expression in a way that it only selects
        /// the fields mentioned in it. It means it won't load the entire Order entity
        /// </summary>
        /// <param name="entity">the Order entity</param>
        /// <param name="price">price to be compared with</param>
        /// <returns></returns>
        [ReplaceWithExpression(MethodName = nameof(HasItemsOverPriceProjection))]
        static public bool HasItemsOverPrice(Order entity, decimal price) => 
            HasItemsOverPriceProjection().Compile().Invoke(entity, price);
        static Expression<Func<Order, decimal, bool>> HasItemsOverPriceProjection() =>
            (entity, price) => entity.Items.Any(i => i.Price > price);
    }
}