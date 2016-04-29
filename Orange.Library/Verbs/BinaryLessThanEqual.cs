using Orange.Library.Managers;

namespace Orange.Library.Verbs
{
	public class BinaryLessThanEqual : ComparisonVerb
	{
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
				return "Less than equal";
			}
		}

		public override string ToString()
		{
			return "<=";
		}
	}
}