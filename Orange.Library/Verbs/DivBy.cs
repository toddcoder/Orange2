using Core.Assertions;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
   public class DivBy : TwoValueVerb
   {
      public override VerbPrecedenceType Precedence => VerbPrecedenceType.Divide;

      public override Value Evaluate(Value x, Value y)
      {
         var divisor = y.Number;
         divisor.Must().Not.Equal(0).OrThrow(Location, () => "Divide by 0");

         return x.Number % divisor == 0;
      }

      public override string Location => "Divide by?";

      public override string Message => "isDiv";

      public override string ToString() => "/?";
   }
}