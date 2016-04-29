using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class ApplyRepeat : Verb
	{
		public override Value Evaluate()
		{
			Value value = Runtime.State.Stack.Pop(true, "");
			return value.Do(true);
		}

		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.Invoke;
			}
		}
	}
}