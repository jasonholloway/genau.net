using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Genau
{
    public interface IEagerEnumerable<V> : IEnumerable<V> {}
    public interface IEagerOrderedEnumerable<V> : IEagerEnumerable<V>, IOrderedEnumerable<V> {}

    public class EagerEnumerable<V> : IEagerEnumerable<V>
    {
        V[] _items;

        internal EagerEnumerable(V[] items) {
            _items = items;
        }

        public IEnumerator<V> GetEnumerator()
            => _items.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    public static class EagerEnumerable 
    {
        public static IEagerEnumerable<V> From<V>(IEnumerable<V> enumerable)
            => new EagerEnumerable<V>(enumerable.ToArray());
    }
}
