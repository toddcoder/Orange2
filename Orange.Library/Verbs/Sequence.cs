using Orange.Library.Values;
using Standard.Types.Maybe;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.MessageManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
	public class Sequence : Verb
	{
		public override Value Evaluate()
		{
			var value = State.Stack.Pop(true, "Sequence");
			Object obj;
			return value.As<Object>().Assign(out obj) ? new ObjectSequence(obj) : MessagingState.SendMessage(value, "seq",
            new Arguments());
		}

		public override VerbPresidenceType Presidence => VerbPresidenceType.Apply;

	   public override string ToString() => "seq";
	}
}