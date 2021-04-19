using Core.Assertions;
using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
   public class IntegerDivide : TwoValueVerb
   {
      public override ExpressionManager.VerbPrecedenceType Precedence => ExpressionManager.VerbPrecedenceType.Divide;

      public override Value Evaluate(Value x, Value y)
      {
         var divisor = y.Number;
         divisor.Must().Not.BeZero().OrThrow(Location, () => "Divisor is 0");

         return (int)x.Number / (int)divisor;
      }

      public override string Location => "Integer divide";

      public override string Message => "intDiv";

      public override string ToString() => "./";
   }
}