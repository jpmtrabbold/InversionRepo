using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace InversionRepo.Extensions
{
    public static partial class EfExtensions
    {
        static Dictionary<string, List<string>> includePaths { get; set; } = new Dictionary<string, List<string>>();

        public static IQueryable<T> Include<T>(this IQueryable<T> source, List<string> navigationPropertyPaths)
            where T : class
        {
            return navigationPropertyPaths.Aggregate(source, (query, path) => query.Include(path));
        }

        public static List<string> GetIncludePaths(this DbContext context, Type clrEntityType)
        {
            if (includePaths == null)
                throw new Exception("includePaths can't be null");

            if (clrEntityType == null)
                throw new Exception("clrEntityType can't be null");

            var typeName = clrEntityType.FullName;

            if (string.IsNullOrWhiteSpace(typeName))
                throw new Exception("clrEntityType.FullName can't be null");

            try
            {
                // cache
                lock (includePaths)
                {
                    if (includePaths.ContainsKey(typeName))
                        return includePaths[typeName];

                    var paths = new List<string>();

                    var entityType = context.Model.FindEntityType(clrEntityType);
                    var includedNavigations = new HashSet<INavigation>();
                    var stack = new Stack<IEnumerator<INavigation>>();
                    var count = 0;
                    while (count < 10000)
                    {
                        count++;
                        var entityNavigations = new List<INavigation>();
                        foreach (var navigation in entityType.GetNavigations())
                        {
                            if (includedNavigations.Add(navigation))
                                entityNavigations.Add(navigation);
                        }
                        if (entityNavigations.Count == 0)
                        {
                            if (stack.Count > 0)
                                paths.Add(string.Join(".", stack.Reverse().Select(e => e.Current.Name)));
                        }
                        else
                        {
                            foreach (var navigation in entityNavigations)
                            {
                                var inverseNavigation = navigation.FindInverse();
                                if (inverseNavigation != null)
                                    includedNavigations.Add(inverseNavigation);
                            }
                            stack.Push(entityNavigations.GetEnumerator());
                        }
                        while (stack.Count > 0 && !stack.Peek().MoveNext())
                            stack.Pop();
                        if (stack.Count == 0) break;
                        entityType = stack.Peek().Current.GetTargetType();
                    }
                    includePaths.Add(typeName, paths);
                    return paths;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception on GetIncludePaths for type {typeName}. Please check the inner exception for more details.", ex);
            }
        }

    }
}