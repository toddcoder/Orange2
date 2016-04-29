using Orange.Library.Values;
using Standard.Types.Maybe;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
	public class AppendToSet : Verb
	{
		const string LOCATION = "Append to set";

		public override Value Evaluate()
		{
			var stack = Runtime.State.Stack;
			var y = stack.Pop(true, LOCATION);
			Set set;
			if (stack.IsEmpty)
			{
				set = new Set(new[]
				{
					y
				});
				return set;
			}
			var x = stack.Pop(true, LOCATION);
			if (x.As<Set>().Assign(out set))
			{
				set.Add(y);
				return set;
			}
			set = new Set(new[]
			{
				x,
				y
			});
			return set;
		}

		public override VerbPresidenceType Presidence => VerbPresidenceType.CreateArray;

	   public override string ToString() => "^^";
	}
}