using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class PushElse : Verb
	{
		const string LOCATION = "Push else";

		public override Value Evaluate()
		{
			ValueStack stack = Runtime.State.Stack;
			var elseIf = stack.Pop<Values.If>(false, LOCATION);
			var _if = stack.Pop<Values.If>(false, LOCATION);
			_if.Next = elseIf;
			return _if;
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
			return "elseif";
		}
	}
}