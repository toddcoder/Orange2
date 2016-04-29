using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
	public class End : Verb, IEnd
	{
		public override Value Evaluate()
		{
			return null;
		}

		public override VerbPresidenceType Presidence => VerbPresidenceType.NotApplicable;

	   public override string ToString() => ";";

	   public bool IsEnd => true;

	   public bool EvaluateFirst => false;

	   public override int OperandCount => 0;
	}
}