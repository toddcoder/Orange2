using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class Min : TwoValueVerb
	{
		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.LessThan;
			}
		}

		public override Value Evaluate(Value x, Value y)
		{
			return x.Compare(y) < 0 ? x : y;
		}

		public override string Location
		{
			get
			{
				return "Upper limit";
			}
		}

		public override string Message
		{
			get
			{
				return "min";
			}
		}

		public override string ToString()
		{
			return "min";
		}
	}
}