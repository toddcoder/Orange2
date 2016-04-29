using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class PushFieldSeparator : Verb
	{
		public override Value Evaluate()
		{
			return Runtime.State.FieldSeparator;
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
			return ",";
		}
	}
}