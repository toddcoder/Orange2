using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class NewMessage : Verb
	{
		const string LOCATION = "New Message";

		public override Value Evaluate()
		{
			ValueStack stack = Runtime.State.Stack;
			Value member = stack.Pop(true, LOCATION);
			Value target = stack.Pop(true, LOCATION);
			return Runtime.SendMessage(target, "newMessage", member);
		}

		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.Statement;
			}
		}

		public override string ToString()
		{
			return "=:";
		}
	}
}