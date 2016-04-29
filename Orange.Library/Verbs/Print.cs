using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class Print : OneValueVerb
	{
		public override Value Evaluate(Value value)
		{
			Runtime.State.Print(value.Text);
			return null;
		}

		public override string Location
		{
			get
			{
				return "Print";
			}
		}

		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.NotApplicable;
			}
		}
	}
}