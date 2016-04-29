using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class IsVal : TwoValueVerb
	{
		public override Value Evaluate(Value x, Value y)
		{
			return MessageManager.MessagingState.SendMessage(x, "val?", new Arguments(y));
		}

		public override string Location
		{
			get
			{
				return "Is value";
			}
		}

		public override string Message
		{
			get
			{
				return "is-val";
			}
		}

		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.Equals;
			}
		}
	}
}