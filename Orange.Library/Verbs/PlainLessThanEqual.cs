using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class PlainLessThanEqual : ComparisonVerb
	{
		Value x;
		Value y;

		public PlainLessThanEqual(Value x, Value y)
		{
			this.x = x;
			this.y = y;
		}

		public override Value Evaluate()
		{
			return DoComparison(x, y);
		}

		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.LessThanEqual;
			}
		}

		public override bool Compare(int comparison)
		{
			return comparison <= 0;
		}

		public override string Location
		{
			get
			{
				return "plain less than equal";
			}
		}
	}
}