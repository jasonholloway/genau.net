using System;
using Xunit;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using static Genau.Gen2;
using static System.Linq.Enumerable;
using System.Threading;
using System.Collections;

namespace Genau
{
    public class GenTests
    {

        public class GenBool 
        {
            [Fact]
            public void GensMany()
                => RunMany(100).DistinctValues().ShouldBe(2);

            [Fact]  //should test per contextual seed
            public void WhenSeeded_AlwaysTheSame()
                => WithSeed(12, () => RunMany(10))
                    .ShouldBe(WithSeed(12, () => RunMany(10)));
                    
            [Fact]
            public void WhenNotSeeded_AlwaysDifferent()
                => RunMany(10)
                    .ShouldNotBe(RunMany(10));

            bool Run() => GenBool();

            IEnumerable<bool> RunMany(int times)
                => Range(0, times).Select(_ => Run());
        }


        public class GenNatural 
        {            
            [Fact]
            public void GensMany()
                => RunMany(100).DistinctValues().ShouldBeGreaterThan(50);

            [Fact]
            public void GensOnlyNaturals()
                => RunMany(100).ShouldAllBe(i => i >= 0);
            
            [Fact]  //should test per contextual seed
            public void WhenSeeded_AlwaysTheSame()
                => WithSeed(123, () => RunMany(10))
                    .ShouldBe(WithSeed(123, () => RunMany(10)));

            [Fact]
            public void WhenNotSeeded_AlwaysDifferent()
                => RunMany(10)
                    .ShouldNotBe(RunMany(10));

            int Run() => GenNatural();

            IEnumerable<int> RunMany(int times)
                => Range(0, times).Select(_ => Run()); 
        }

    }

    //so we have a problem with laziness, especially in Enuemrables
    //the ambient scope will change in the execution of an enumerable's iterator.
    //which buggers us up, doesn't it?
    //indeed it does. For this to work we need eager computations.
    //unless we could by some bizarre magic make enumerations eager by default just here.

    //Even if we just gave out EagerEnumerables, any unexpected use of standard LINQ 
    //
    //The problem is not just in making a context available - it's in the indeterminate laziness.

    //We could somehow scan our generators for calls to lazy LINQ
    //but even then, other lazy mechanisms might exist

    //We could test from the top inclusively
    //Every generator used deterministically (ie with a seed)
    //would be repeatedly called to ensure the identity of its results

    public interface IEagerEnumerable<T> {}
    public class EagerEnumerable<T> : IEagerEnumerable<T>, IEnumerable<T>, IEnumerator<T>
    {
        public T Current => throw new NotImplementedException();

        object IEnumerator.Current => throw new NotImplementedException();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool MoveNext()
        {
            //the enumerator could capture a seed on its construction, taken from its context
            //BUT! there's no great way to propagate this seed to successor clauses
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }


    public static class Gen2 
    {
        static AsyncLocal<Random> _random = new AsyncLocal<Random>();

        static Random Random => _random.Value ?? new Random();

        public static V WithSeed<V>(int seed, Func<V> fn) {
            try {
                _random.Value = new Random(seed);
                return fn();
            }
            finally {
                _random.Value = null;
            }
        }


        public static bool GenBool()
            => Random.Next(3) > 1;

        public static int GenNatural()
            => Random.Next(100);
    }


    public static class Extensions {

        public static ISet<V> ToSet<V>(this IEnumerable<V> items)
            => new HashSet<V>(items);

        public static int DistinctValues<V>(this IEnumerable<V> items)
            => items.ToSet().Count();

    }
}
