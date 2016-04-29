using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class ElseIf : Verb
	{
		const string LOCATION = "else if";

		public override Value Evaluate()
		{
			ValueStack stack = Runtime.State.Stack;
			Value result = stack.Pop(true, LOCATION, false);
			Value condition = stack.Pop(true, LOCATION);
			return new Values.When(condition, result);
		}

		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.When;
			}
		}

		public override string ToString()
		{
			return "elseif";
		}
	}
}