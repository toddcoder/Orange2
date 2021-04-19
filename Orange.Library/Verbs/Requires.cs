using Core.Assertions;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
   public class Requires : Verb
   {
      protected const string LOCATION = "Require";

      public override Value Evaluate()
      {
         var stack = State.Stack;
         var y = stack.Pop(true, LOCATION);
         var x = stack.Pop(true, LOCATION);
         Case.Match(x, y, false, null).Must().BeTrue().OrThrow(LOCATION, () => $"{x} doesn't match {y} requirement");

         return x;
      }

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Apply;

      public override string ToString() => "requires";
   }
}