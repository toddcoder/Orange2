using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
   public abstract class GeneratorTermination : Verb
   {
      public override Value Evaluate()
      {
         var stack = State.Stack;
         var right = stack.Pop(true, Location);
         var left = stack.Pop(true, Location);
         var generator = Assert(left.PossibleGenerator(), Location, $"{left} is not a generator");

         if (right is Lambda lambda)
         {
            var iterator = new NSIterator(generator);
            return Evaluate(iterator, lambda);
         }

         Throw(Location, $"{right} is not a lambda");
         return null;
      }

      public abstract Value Evaluate(NSIterator iterator, Lambda lambda);

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Apply;

      public abstract string Location { get; }

      public override string ToString() => Location;
   }
}