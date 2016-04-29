using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class Cast : Verb
	{
		const string STR_LOCATION = "Cast";

		public override Value Evaluate()
		{
			ValueStack stack = Runtime.State.Stack;
			Value source = stack.Pop(true, STR_LOCATION);
			Value target = stack.Pop(true, STR_LOCATION);
			return MessageManager.MessagingState.SendMessage(target, "cast", new Arguments(source));
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
			return "!!";
		}
	}
}