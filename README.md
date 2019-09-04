# InversionRepo
A generic repository for EntityFrameworkCore - **but** flexible and performant (for a generic repo)

## Dependencies
- .NET Core
- EntityFrameworkCore 2.2.6

## The Problem
I had to take on a new project for building a Web Application and, as always, I had my 'Analysis paralysis' moment while deciding which data access pattern I would use.
The usual opinions are:
1- your business layer shouldn't touch your context (because developers are dumb?), so you should protect it with your life (through a repo layer)
2- EF is, in itself, a data access abstraction, so it's all good for your business layer to use your context

Well, I admit I like approach 2 better, but I feel like it adds so much clutter into the business layer. Whereas approach 1 hides all the clutter, it becomes inflexible and underperformant (because you end up having to get all fields from an entity, for example)

I also believe that automatic mapping solutions between Entities and DTOs are not good architectural practices. So if we are writing our mappings manually, **we better make them reusable, right?**

## The Solution
InversionRepo wraps your DbContext and provides:
- the context itself whenever you need it (so we don't have to reivent the wheel and create endless methods to mimic Entity Framework)
- useful, flexible and performant methods to select data directly to your DTOs using the power of reusable **Expressions** - thanks Ben Cull for the [inspiration.](https://benjii.me/2018/01/expression-projection-magic-entity-framework-core/)

## How to setup
While this is still not a NuGet package:
- Copy the InversionRepo project into your project (leave the unit tests out if you want)
- Assumption: you already added a DbContext to your Web project
- Import dependencies into the file where you inject (usually StartUp.cs): `using InversionRepo.Interfaces;` and `using InversionRepo;`
- Add a scoped service like this: `services.AddScoped<IRepository<MyDbContext>, Repository<MyDbContext>>();`
- Add parameters to the constructors in which classes you want to access your data:
```
public class MyBusinessService : BaseService
{
    IRepository<MyDbContext> _repo
    public MyBusinessService(IRepository<MyDbContext> repo) : base(repo)
    {
        _repo = repo
    }
    ...
}
```

- Now you are ready to use _repo. **In the Unit Tests project there are very good examples on how to setup the Entities, DTOs, mapping expressions and usage of `ProjectedList` and `ProjectedListBuilder`**

## How to use `ProjectedList` and variants

### ProjectedGetById
Let's say your API caller requested an Order by id:
```
client.getOrderById(42) // pseudo-code here
```

If you are being a good boy, you want to return a DTO to your caller (instead of exposing your data model and probably giving unnecessary data). So that's how your method will look like (for the sake of this example there are no layer separations):
```
async public Task<OrderDto> Get(int orderId)
{
    return await _repo.ProjectedGetById(orderId, OrderDto.ProjectionFromEntity());
}
```

And that's it. But how `ProjectedGetById` knows what table to select data from, what is the Dto type, and how to map the fields? All of this in inferred from `OrderDto.ProjectionFromEntity()`. So now I'm gonna show you my OrderDto:
```
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
```

Alright. The `ProjectionFromEntity` method returns an [**expression**](https://benjii.me/2018/01/expression-projection-magic-entity-framework-core/) that takes in a `Order` object (that's how it knows which table to select data from) and spits out a `OrderDto` object, with the mapping you see above. By the way, this mapping is:
- totally reusable
- very performant, as Entity Framework only selects the fields that are explicitly mentioned in the expression
- composable - you can refer to sub-projections indefinitely (as in `Items` and `HasItemsWithPriceOver100` property assignments), and Entity Framework will only select the data you mentioned.
Just to give you the full picture, the Order class:
```
public partial class Order : BaseEntity
{
    public int? CustomerId { get; set; }
    public Customer Customer { get; set; }
    public int DeliveryAddressId { get; set; }
    public Address DeliveryAddress { get; set; }
    public ICollection<OrderItem> Items { get; private set; } = new HashSet<OrderItem>();
}
```

And the other partial of Order:
```
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
```

This is another nice surprise (some will find it ugly, totally understandable). It relies on a modified version of LinqKit's `AsExpandable`. You can learn more about it [**here**](https://benjii.me/2018/01/expression-projection-magic-entity-framework-core/). Long story short: the method `HasItemsOverPrice` can be called **inside** a projection, and it won't make Entity Framework load the entire database (ok a bit of drama here). It will respect the expression tree and only select the data that is mentioned in that sub-expression. It makes entity business rules much easier to be reused across normal code and expression code (if you can get past the ugly sintax - but you don't need to use this part of the library if you don't want).

Ok, now going back a bit to `OrderDto.ProjectionFromEntity`:
```
Items = entity.Items.AsQueryable().Select(OrderItemDto.ProjectionFromEntity()).ToList(),
```

Let's check `OrderItemDto` out?
```
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
```

As I promised, reusable and composable mappings. Not only OrderItemDto`s expression is being used inside OrderDto's expression, but it is also calling ProductDiscountDto's one, further down a level.

I think, by now, if you did your homework, you realized how powerful this whole concept can be.

### ProjectedGet
This uses pretty much the same concept, but you can pass on a `where` predicate instead of an id. Some examples:
```
var ordersWithPriceAbove90 = await _repo.ProjectedList(
    OrderDto.ProjectionFromEntity(), 
    o => Order.HasItemsOverPrice(o, 90));

var ordersWithAtLeastOneProductWithMoreThan10PercentOfDiscount = 
    await _repo.ProjectedList(
    OrderDto.ProjectionFromEntity(), 
    o => o.Items.Any(i => i.Product.Discounts.Any(d => d.DiscountFactor > 0.10M)));

```


### ProjectedListBuilder
This is for the more complex cases where you need a bit more flexibility. Let's say the client is requesting a paginated list:
```
//pseudo-code again (actually javascript haha)
const listRequest = {
    pageSize: 10
    pageNumber: 2
    sortField: 'customerName'
    sortOrderAscending: true
}
const searchTerm = 'blablabla'
const minimumPrice = 90
client.getOrdersList(searchTerm, minimumPrice, listRequest) // if this doesn't look like an API call to you, you gotta start using NSwagStudio right now
```

My API method will be something like (probably your DTO for list situations should be different, but you get the idea):
```
async public Task<OrderListResponse> Get(string searchTerm, decimal? minimumPrice, ListRequest listRequest)
{
    var builder = _repo.ProjectedListBuilder(OrderDto.ProjectionFromEntity(), listRequest)
        .OrderByDescending(d => d.Id)
        .ConditionalOrder("customerName", d => d.Customer.Name)
    
    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        builder.Where(d => d.Customer.Name.Contains(searchTerm));
    }

    if (minimumPrice.HasValue)
    {
        builder.Where(d => Order.HasItemsOverPrice(d, minimumPrice)); // see how it is reusable? Up to you if the convenience outweighs the ugly sintax)
    }

    return new OrderListResponse
    {
        Orders = await builder.ExecuteAsync(), // List<OrderDto>
        TotalRecords = await builder.CountAsync() // int
    };
}
```


## How to access DbContext
That's really simple:
```
_repo.Context;
```

Full context access - because I trust you as a developer (do not mess this up ok?)

## Other Cases
The repo also provides helper methods for the most common situations (which also could be addressed directly through the use of _repo.Context whenever these are not enough):
- `_repo.Set<Order>()` - gives the DbContext Set for that type
- `_repo.GetById<Order>(23)` - gets the EF tracked `entity` of Order with id 23, so you can update it and save it back to the database with `_repo.SaveEntity(entity)`
- `_repo.Get<Order>(o => o.CustomerId = 2)` - same as GetById, but using a where predicate
- `_repo.Remove(entity)` - self-explanatory, and then you have to call `_repo.SaveEntity(entity)` to commit
- `_repo.LoadCollection()` and `_repo.LoadReference()` for loading sub-entities (in EF Tracked situations...for projections you should use the ProjectedGet variants)