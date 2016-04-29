using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class MatchResult : Verb
	{
		const string STR_LOCATION = "Match result";

		public override Value Evaluate()
		{
/*			Value match = RegionManager.State[Runtime.VAL_MATCH_VALUE];
			Runtime.Assert(match.Type == Value.ValueType.Match, STR_LOCATION, "Match not defined");
			return ((Match)match).Result();*/
			return null;
		}

		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.Apply;
			}
		}

		public override string ToString()
		{
			return "<|";
		}
	}
}