using Orange.Library.Managers;
using Orange.Library.Values;

namespace Orange.Library.Verbs
{
	public class Inside : Verb
	{
		const string STR_LOCATION = "Inside";

		public override Value Evaluate()
		{
			Value arrayValue = Runtime.State.Stack.Pop(true, STR_LOCATION);
			Value value = Runtime.State.Stack.Pop(true, STR_LOCATION);
			if (arrayValue.IsArray)
			{
				var array = (Array)arrayValue.SourceArray;
				return array.ContainsValue(value);
			}
			return arrayValue.Text.IndexOf(value.Text, System.StringComparison.Ordinal) > -1;
		}

		public override ExpressionManager.VerbPresidenceType Presidence
		{
			get
			{
				return ExpressionManager.VerbPresidenceType.Equals;
			}
		}
	}
}