using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class EmptyArray : Verb
	{
		public override Value Evaluate() => new Array();

	   public override ExpressionManager.VerbPrecedenceType Precedence => ExpressionManager.VerbPrecedenceType.Push;

	   public override string ToString() => "<>";
	}
}