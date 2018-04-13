using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;

namespace Genau
{
    public class Adapted<T>
    {
        public static implicit operator Adapted<T>(T val)
            => null;
    }

    public static class Gen
    {
        //public static Gen<T> From<T>(Expression<Func<T>> fn)
        //{
        //    return new Gen<T>(null);
        //}

        public static Gen<T> From<T>(T v) => new Gen<T>(() => default(T));
        public static Gen<T> From<T>(Func<T> fn) => null;
        public static Func<A1, Gen<T>> From<A1, T>(Func<A1, T> fn) => null;
        public static Func<A1, A2, Gen<T>> From<A1, A2, T>(Func<A1, A2, T> fn) => null;


        public static Constraint Constraint() => null;



        public static Gen<V> Pick<V>(this ISource<V> source)
            => null;


        public static Gen<T> Pick<T>(params T[] candidates)
            => null;
        
	public static bool Hello() => true;

        public static void Set<T>(string name, T value) { }
        public static T Get<T>(string name) => default(T);

    }

    

    public class Constraint
    {

    }


    public class Gen<T> : ISource<T>
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


        public T Value() => default(T);


        private Func<T> Fn => _fn.Compile();
    }
       

    
    public class ScopedValues<T> : ISource<T>
    {
        public T Value() => default(T);

        public static implicit operator T(ScopedValues<T> var) 
            => var.Value();
    }



    public interface ISource<V> { }


    public static class Pick
    {
        public static V[] Some<V>(params V[] candidates) => null;
        public static V[] Some<V>(Func<V> factory) => null;
    }


    public class Animal { }

    public class Cage { }
    
    public class Hamster : Animal
    {
        public string Name;
        public bool HasFur;
    }
    
    public class PetShop
    {
        public Animal[] Animals;
        public Cage[] Cages;
    }


    public class Class1
    {
        void BlahBlah()
        {
            var genName = Gen.From("Hammy");

            var genHamster = Gen.From(
                () => new Hamster {
                            Name = genName.Map(s => s.Trim()),
                            HasFur = true
                        });


            var hamstersPerShop = Gen.Constraint();


            var likelyNames = new ScopedValues<string>();


            Hamster genHammy(params string[] suitableNames) 
                => new Hamster
                {
                    Name = Gen.Pick(suitableNames),
                    HasFur = Gen.Pick(true, false)
                };

            PetShop genPetShop()
                => new PetShop
                {
                    Animals = Pick.Some(() => genHammy("Edward", "Tarquin"))
                };


            //can't pick without a context!
            //should throw an immediate exception in that case
            //similarly should throw if nested

            //scale should be specified at top, but then occasionally used underneath too
            //scale can be cascaded down by arguments

            

            //and now - how to generate relations between things, as constraints?
            //the constraint shouldn't be declared by attribute, as such an attribute would need to be repeated for it to be valid.
            //athough this would also allow constraints to be added to piecemeal.
            

            
            Hamster blah = genHamster.Value();
        }
    }
}
