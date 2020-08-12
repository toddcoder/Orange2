using Core.DataStructures;
using Core.Monads;
using static Core.Monads.MonadFunctions;
using static Orange.Library.Values.Nil;

namespace Orange.Library.Values
{
   public class FlatGenerator : NSGenerator
   {
      INSGenerator sourceGenerator;
      MaybeQueue<NSIterator> iterators;
      IMaybe<NSIterator> currentIterator;

      public FlatGenerator(INSGenerator sourceGenerator)
         : base(null)
      {
         this.sourceGenerator = sourceGenerator;
         iterators = new MaybeQueue<NSIterator>();
         currentIterator = none<NSIterator>();
      }

      public void Add(Value value)
      {
         var iterator = new NSIterator(value.PossibleGenerator().DefaultTo(() => new NSOneItemGenerator(value)));
         iterators.Enqueue(iterator);
      }

      public override void Reset()
      {
         var sourceIterator = new NSIterator(sourceGenerator);
         foreach (var item in sourceIterator)
         {
            Add(item);
         }
      }

      public override Value Next()
      {
         if (currentIterator.If(out var iterator))
         {
            var value = iterator.Next();
            if (value.IsNil)
            {
               currentIterator = none<NSIterator>();
            }
            else
            {
               return value;
            }
         }

         while (iterators.Count > 0)
         {
            currentIterator = iterators.Dequeue();
            if (currentIterator.If(out iterator))
            {
               iterator.Reset();

               var value = iterator.Next();
               if (!value.IsNil)
               {
                  return value;
               }
            }
            else
            {
               return NilValue;
            }
         }

         return NilValue;
      }

      public override string ToString() => sourceGenerator.ToString();
   }
}