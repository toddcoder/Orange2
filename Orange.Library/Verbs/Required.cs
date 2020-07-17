using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class Required : Verb
	{
		const string LOCATION = "Required";

		public override Value Evaluate() => When.EvaluateWhen(true, LOCATION);

	   public override ExpressionManager.VerbPrecedenceType Precedence => ExpressionManager.VerbPrecedenceType.Apply;

	   public override string ToString() => "Required";
	}
}