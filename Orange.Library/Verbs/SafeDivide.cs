using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class SafeDivide : TwoValueVerb
	{
		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.Divide;
			}
		}

		public override Value Evaluate(Value x, Value y)
		{
			return y.Number == 0 ? 0 : x.Number / y.Number;
		}

		public override string Location
		{
			get
			{
				return "Safe Divide";
			}
		}

		public override string Message
		{
			get
			{
				return "sdiv";
			}
		}

		public override string ToString()
		{
			return "//";
		}
	}
}