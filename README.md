# InversionRepo
A generic repository for EntityFrameworkCore - **but** flexible and performant

## Dependencies
- .NET Core
- EntityFrameworkCore 2.2.6

## The Problem
I had to take on a new project for building a Web Application and, as always ,I had my 'Analysis paralysis' moment while deciding which data access pattern I would use.
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
- Now you are ready to use _repo. My favourite methods are:
    - `ProjectedList`
    - `ProjectedListBuilder`
    - `ProjectedGetById`
- I know...I should explain it better. For now, while the documentation is not finished yet, please feel free to look into the code and check how it works (Pretty much `Repository.cs` and `ListRequestBuilder.cs`).
-  **In the Unit Tests project there are very good examples on how to set the Entities, DTOs, mapping expressions and usage of `ProjectedList` and `ProjectedListBuilder`**

## How to use `ProjectedList` and variants
Let's say your API caller requested an Order by id:
```
client.getOrderById(42) // pseudo-code here
```
continues...

