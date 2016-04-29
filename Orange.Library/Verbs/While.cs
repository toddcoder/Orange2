using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
	public class While : Verb
	{
		public override Value Evaluate()
		{
			var value = Runtime.State.Stack.Pop(true, "While");
			return new MessageData("while", value);
		}

		public override VerbPresidenceType Presidence => VerbPresidenceType.PreIncrement;

	   public override string ToString() => "while";
	}
}