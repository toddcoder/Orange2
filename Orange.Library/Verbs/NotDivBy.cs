using Core.Assertions;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
   public class NotDivBy : TwoValueVerb
   {
      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Divide;

      public override Value Evaluate(Value x, Value y)
      {
         var divisor = y.Number;
         divisor.Must().Not.BeZero().OrThrow(Location, () => "Divide by 0");

         return x.Number % divisor != 0;
      }

      public override string Location => "Not div by";

      public override string Message => "isNotDiv";

      public override string ToString() => "!/?";
   }
}