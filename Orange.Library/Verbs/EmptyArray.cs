using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class EmptyArray : Verb
	{
		public override Value Evaluate()
		{
			return new Array();
		}

		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.Push;
			}
		}

		public override string ToString()
		{
			return "<>";
		}
	}
}