using System;
using Xunit;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using static Genau.Gen2;
using static System.Linq.Enumerable;

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
            public void IsDeterministic()
                => WithSeed(12, () => Run())
                    .ShouldBe(WithSeed(12, () => Run()));

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
            

            int Run() => GenNatural();

            IEnumerable<int> RunMany(int times)
                => Range(0, times).Select(_ => Run());                
        }

    }


    public static class Gen2 
    {

        public static V WithSeed<V>(int seed, Func<V> fn) => default(V);


        public static bool GenBool()
            => new Random().Next(3) > 1;

        public static int GenNatural()
            => new Random().Next(100);
    }


    public static class Extensions {

        public static ISet<V> ToSet<V>(this IEnumerable<V> items)
            => new HashSet<V>(items);

        public static int DistinctValues<V>(this IEnumerable<V> items)
            => items.ToSet().Count();

    }
}
