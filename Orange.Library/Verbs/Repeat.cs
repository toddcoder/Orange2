using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class Repeat : TwoValueVerb
	{
		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.Repeat;
			}
		}

		public override Value Evaluate(Value x, Value y)
		{
			return MessageManager.MessagingState.SendMessage(x, Message, new Arguments(y));
		}

		public override string Location
		{
			get
			{
				return "Repeat";
			}
		}

		public override string Message
		{
			get
			{
				return "repeat";
			}
		}

		public override string ToString()
		{
			return "><";
		}

		public override bool UseArrayVersion
		{
			get
			{
				return false;
			}
		}
	}
}