using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class ExitSignal : Verb, IEnd
	{
		public override Value Evaluate()
		{
			ValueStack stack = Runtime.State.Stack;
			if (stack.IsEmpty)
			{
				Runtime.State.ExitSignal = true;
				return null;
			}
			Value value = stack.Pop(true, "Exit");
			if (value.IsNil)
				return null;
			if (value.IsTrue)
				Runtime.State.ExitSignal = true;
			return null;
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
			return "exit";
		}

		public bool IsEnd
		{
			get
			{
				return true;
			}
		}

		public bool EvaluateFirst
		{
			get
			{
				return true;
			}
		}
	}
}