using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class Required : Verb
	{
		const string LOCATION = "Required";

		public override Value Evaluate()
		{
			return When.EvaluateWhen(true, LOCATION);
		}

		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.Apply;
			}
		}

		public override string ToString()
		{
			return "Required";
		}
	}
}