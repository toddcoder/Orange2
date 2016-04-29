using Orange.Library.Values;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
   public class Cons : Verb
   {
      const string LOCATION = "Cons";

      public override Value Evaluate()
      {
         var stack = State.Stack;
         var right = stack.Pop(true, LOCATION);
         var left = stack.Pop(true, LOCATION);
         var list = right.As<List>();
         if (list.IsSome)
            return new List(left, list.Value);
         return new List(left, right);
      }

      public override VerbPresidenceType Presidence => VerbPresidenceType.CreateArray;

      public override bool LeftToRight => false;

      public override string ToString() => "::";
   }
}