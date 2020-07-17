using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class Comma : Verb
	{
		public override Value Evaluate() => null;

	   public override ExpressionManager.VerbPrecedenceType Precedence => ExpressionManager.VerbPrecedenceType.NotApplicable;

	   public override string ToString() => ";";
	}
}