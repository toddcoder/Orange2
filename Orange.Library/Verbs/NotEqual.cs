using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
	public class NotEqual : ComparisonVerb
	{
		public override bool Compare(int comparison) => comparison != 0;

	   public override string ToString() => "!=";

	   public override VerbPrecedenceType Precedence => VerbPrecedenceType.NotEqual;

	   public override string Location => "Not equal";
	}
}