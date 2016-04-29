using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class Otherwise : Verb
	{
		const string LOCATION = "Otherwise";

		public override Value Evaluate()
		{
			ValueStack stack = Runtime.State.Stack;
			Value block = stack.Pop(true, LOCATION, false);
			Value value = stack.Pop(true, LOCATION);
			var when = value as Values.When;
			if (when == null)
				return value;
			when.Otherwise = block;
			return when;
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
			return "=!";
		}
	}
}