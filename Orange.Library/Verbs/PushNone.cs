using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
   public class PushNone : Verb
   {
      public override Value Evaluate() => new None();

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Push;

      public override string ToString() => "$[]";
   }
}