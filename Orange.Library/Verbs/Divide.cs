using Core.Assertions;
using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
	public class Divide : TwoValueVerb
	{
		public override Value Evaluate(Value x, Value y)
		{
			var divisor = y.Number;
         divisor.Must().Not.Equal(0).OrThrow(Location, () => "Divide by 0");

			return x.Number / divisor;
		}

		public override string Location => "Divide";

	   public override string Message => "div";

	   public override string ToString() => "/";

	   public override VerbPrecedenceType Precedence => VerbPrecedenceType.Divide;
	}
}