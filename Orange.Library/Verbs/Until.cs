using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class Until : Verb
	{
		public override Value Evaluate()
		{
			var value = Runtime.State.Stack.Pop(true, "Until");
			return new MessageData("until", value);
		}

		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.PreIncrement;
			}
		}

		public override string ToString()
		{
			return "until";
		}
	}
}