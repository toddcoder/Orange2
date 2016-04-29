using Orange.Library.Values;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
   public class Scan : Verb
   {
      const string LOCATION = "Scan";

      public override Value Evaluate()
      {
         var stack = State.Stack;
         var y = stack.Pop(true, LOCATION);
         var lambda = y.As<Lambda>();
         Assert(lambda.IsSome, LOCATION, "Lambda required as right-hand argument");
         var x = stack.Pop(true, LOCATION);
         var source = x.As<INSGeneratorSource>();
         Assert(source.IsSome, LOCATION, "Left hand side must provide generator source");
         return new Scanner(source.Value, lambda.Value);
      }

      public override VerbPresidenceType Presidence => VerbPresidenceType.Apply;

      public override string ToString() => "<>";
   }
}