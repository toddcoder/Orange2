using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class Swap : Verb
	{
		const string LOCATION = "Swap";

		public override Value Evaluate()
		{
			ValueStack stack = Runtime.State.Stack;
			var variable2 = stack.Pop<Variable>(false, LOCATION);
			var variable1 = stack.Pop<Variable>(false, LOCATION);
			Value value1 = variable1.Value;
			Value value2 = variable2.Value;
			variable1.Value = value2;
			variable2.Value = value1;
			return variable1;
		}

		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.Statement;
			}
		}

		public override bool LeftToRight
		{
			get
			{
				return false;
			}
		}

		public override string ToString()
		{
			return "<->";
		}
	}
}