using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class Mod : TwoValueVerb
	{
		public override Value Evaluate(Value x, Value y) => x.Number % y.Number;

	   public override string Location => "Modulo";

	   public override string Message => "mod";

	   public override string ToString() => "%";

	   public override ExpressionManager.VerbPresidenceType Presidence => ExpressionManager.VerbPresidenceType.Mod;
	}
}