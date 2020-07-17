using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
	public class Equals : ComparisonVerb
	{
		public override bool Compare(int comparison) => comparison == 0;

	   public override string ToString() => "==";

	   public override VerbPrecedenceType Precedence => VerbPrecedenceType.Equals;

	   public override string Location => "Equals";
	}
}