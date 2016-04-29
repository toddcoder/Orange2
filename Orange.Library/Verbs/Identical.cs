using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class Identical : Verb
	{
		const string LOCATION = "Identical";

		public override Value Evaluate()
		{
			ValueStack stack = Runtime.State.Stack;
			Value y = stack.Pop(true, LOCATION);
			Value x = stack.Pop(true, LOCATION);
			return x.ID == y.ID;
		}

		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.Equals;
			}
		}

		public override string ToString()
		{
			return "===";
		}
	}
}