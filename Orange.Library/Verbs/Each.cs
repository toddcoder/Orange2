using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class Each : Verb
	{
		const string STR_LOCATION = "Each";
		const string MESSAGE_NAME = "each";

		public override Value Evaluate()
		{
			Value code = Runtime.State.Stack.Pop(true, STR_LOCATION, false);
			Value arrayValue = Runtime.State.Stack.Pop(true, STR_LOCATION);
			if (arrayValue.IsIndexer)
			{
				Arguments arguments = Arguments.FromValue(arrayValue);
				return arguments == null ? arrayValue : MessageManager.MessagingState.SendMessage(arrayValue, MESSAGE_NAME, arguments);
			}
			if (arrayValue.IsArray)
			{
				var array = (Array)arrayValue.SourceArray;
				Arguments arguments = Arguments.GuaranteedExecutable(code);
					return MessageManager.MessagingState.SendMessage(array, MESSAGE_NAME, arguments);
			}
			return arrayValue;
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
			return "-^";
		}
	}
}