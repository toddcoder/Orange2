using Orange.Library.Managers;

namespace Orange.Library.Verbs
{
	public class BinaryLessThan : ComparisonVerb
	{
		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.LessThan;
			}
		}

		public override bool Compare(int comparison)
		{
			return comparison < 0;
		}

		public override string Location
		{
			get
			{
				return "Less than";
			}
		}

		public override string ToString()
		{
			return "<";
		}
	}
}