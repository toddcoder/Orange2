using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class PushCRLF : Verb
	{
		const string LOCATION = "Concatenate CRLF";

		public override Value Evaluate()
		{
			ValueStack stack = Runtime.State.Stack;
			Value y = stack.Pop(true, LOCATION);
			Value x = stack.Pop(true, LOCATION);
			return x.Text + "\r\n" + y.Text;
		}

		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.Concatenate;
			}
		}

		public override string ToString()
		{
			return "'`r`n'";
		}
	}
}