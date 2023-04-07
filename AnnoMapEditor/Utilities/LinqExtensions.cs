using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.Utilities
{
    public static class LinqExtensions
    {
        public static IOrderedEnumerable<TSource> OrderBy<TSource>(this IEnumerable<TSource> source, IComparer<TSource>? comparer)
            => source.OrderBy(s => s, comparer);

        public static (IEnumerable<T>, IEnumerable<T>) SliceHalf<T>(this IEnumerable<T> source)
        {
            var pivot = source.Count() /2;
            return (source.Take(pivot), source.Skip(pivot));
        }
    }
}
