using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
   public class DefaultTo : TwoValueVerb
   {
      public override Value Evaluate(Value x, Value y) => x switch
      {
         Some some => some.Value(),
         None => y,
         _ => x.IsEmpty ? y : x
      };

      public override string Location => "Default to";

      public override string Message => "defaultTo";

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Or;

      public override string ToString() => "??";
   }
}