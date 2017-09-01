using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;

namespace Genau.Net
{
    public static class Gen
    {
        public static Gen<T> From<T>(Expression<Func<T>> fn)
        {
            return new Gen<T>(null);
        }
    }


    public class Gen<T>
    {
        Expression<Func<T>> _fn;

        public Gen(Expression<Func<T>> fn)
        {
            _fn = fn;
        }

        public static implicit operator T(Gen<T> gen)
        {
            //should only coerce when in write context
            return default(T);
        }

        public Gen<T2> Map<T2>(Func<T, T2> map) 
            => new Gen<T2>(() => map(Fn()));            


        public T Generate() => default(T);


        private Func<T> Fn => _fn.Compile();
    }

    public class Hamster
    {
        public string Name;
        public bool HasFur;
    }


    public class Class1
    {
        void BlahBlah()
        {
            var genName = Gen.From(() => "Hammy");

            var genHamster = Gen.From(() => new Hamster {Name = genName.Map(s => s.Trim()), HasFur = true});
            
            //two modes:
            //  1) expanding Expression
            //  2) expanding by weaving

            Hamster blah = genHamster.Generate();
        }
    }
}
