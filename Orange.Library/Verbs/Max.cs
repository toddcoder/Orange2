using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class Max : TwoValueVerb
	{
		public override Value Evaluate(Value x, Value y)
		{
			return x.Compare(y) < 0 ? y : x;
		}

		public override string Location
		{
			get
			{
				return "Max";
			}
		}

		public override string Message
		{
			get
			{
				return "max";
			}
		}

		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.GreaterThan;
			}
		}

		public override string ToString()
		{
			return "max";
		}
	}
}