using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class ConcatenateSeparated : Verb
	{
		const string LOCATION = "Concatenate separated";

		public override Value Evaluate()
		{
			Value y = Runtime.State.Stack.Pop(true, LOCATION);
			Value x = Runtime.State.Stack.Pop(true, LOCATION);
			return x.Text + Runtime.State.FieldSeparator.Text + y.Text;
		}

		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.Concatenate;
			}
		}
	}
}