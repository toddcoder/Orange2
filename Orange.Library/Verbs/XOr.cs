using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class XOr : TwoValueVerb
	{
		public override Value Evaluate(Value x, Value y)
		{
			return x.IsTrue ^ y.IsTrue;
		}

		public override string Location
		{
			get
			{
				return "XOr";
			}
		}

		public override string Message
		{
			get
			{
				return "xor";
			}
		}

		public override string ToString()
		{
			return "xor";
		}

		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.Or;
			}
		}
	}
}