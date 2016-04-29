using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class PushGraph : Verb
	{
		const string LOCATION = "Push graph";

		public override Value Evaluate()
		{
			ValueStack stack = Runtime.State.Stack;
			Value value = stack.Pop(true, LOCATION, false);
			Value nameValue = stack.Pop(false, LOCATION);
			var variable = nameValue as Variable;
			string name = variable == null ? nameValue.Text : variable.Name;
			return new Graph(name, value);
		}

		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.PushGraph;
			}
		}

		public override string ToString()
		{
			return "<-";
		}

		public override bool LeftToRight
		{
			get
			{
				return false;
			}
		}
	}
}