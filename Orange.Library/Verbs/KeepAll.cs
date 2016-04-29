using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class KeepAll : Verb
	{
		const string LOCATION = "Keep all";

		public override Value Evaluate()
		{
			ValueStack stack = Runtime.State.Stack;
			Value y = stack.Pop(true, LOCATION);
			Value x = stack.Pop(true, LOCATION);
			Arguments arguments = Arguments.GuaranteedExecutable(y);
			return Runtime.SendMessage(x, "keep-all", arguments);
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
			return "%-";
		}
	}
}