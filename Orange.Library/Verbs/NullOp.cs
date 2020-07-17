using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
   public class NullOp : Verb
   {
      public override Value Evaluate() => null;

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.NotApplicable;

      public override string ToString() => "...";
   }
}