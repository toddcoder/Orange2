using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
   public class DefaultTo : TwoValueVerb
   {
      public override Value Evaluate(Value x, Value y)
      {
         switch (x)
         {
            case Some some:
               return some.Value();
            case None _:
               return y;
            default:
               return x.IsEmpty ? y : x;
         }
      }

      public override string Location => "Default to";

      public override string Message => "defaultTo";

      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Or;

      public override string ToString() => "??";
   }
}