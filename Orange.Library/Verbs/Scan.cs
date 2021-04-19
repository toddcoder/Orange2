using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
   public class Scan : Verb
   {
      protected const string LOCATION = "Scan";

      public override Value Evaluate()
      {
         var stack = State.Stack;
         var y = stack.Pop(true, LOCATION);
         if (y is not Lambda lambda)
         {
            throw LOCATION.ThrowsWithLocation(() => "Lambda required as right-hand argument");
         }

         var x = stack.Pop(true, LOCATION);
         if (x is not INSGeneratorSource source)
         {
            throw LOCATION.ThrowsWithLocation(() => "Left hand side must provide generator source");
         }

         return new Scanner(source, lambda);
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Apply;

      public override string ToString() => "<>";
   }
}