using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
   public class DoNothing : Verb
   {
      public override Value Evaluate() => null;

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Statement;

      public override string ToString() => "pass";
   }
}