using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class Min : TwoValueVerb
	{
		public override ExpressionManager.VerbPrecedenceType Precedence => ExpressionManager.VerbPrecedenceType.LessThan;

	   public override Value Evaluate(Value x, Value y) => x.Compare(y) < 0 ? x : y;

	   public override string Location => "Upper limit";

	   public override string Message => "min";

	   public override string ToString() => "min";
	}
}