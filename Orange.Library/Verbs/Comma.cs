using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class Comma : Verb
	{
		public override Value Evaluate()
		{
			return null;
		}

		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.NotApplicable;
			}
		}

		public override string ToString()
		{
			return ";";
		}
	}
}