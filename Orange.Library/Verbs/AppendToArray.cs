using Orange.Library.Values;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
	public class AppendToArray : Verb
	{
		const string LOCATION = "Append to array";

		public override Value Evaluate()
		{
			var stack = Runtime.State.Stack;
			var y = stack.Pop(true, LOCATION);
			if (stack.IsEmpty)
			{
				if (y.Type == Value.ValueType.Nil)
					return y;
				return y.Type == Value.ValueType.ArrayBuilder ? y : new Array
				{
					y
				};

			}
			var x = stack.Pop(true, LOCATION);
			if (y.Type == Value.ValueType.Nil)
				return x;
			if (x.Type == Value.ValueType.Nil)
				return y;
		   var list = x.As<InternalList>();
			if (list.IsSome)
			{
				list.Value.Add(y);
				return list.Value;
			}
			return x.Type == Value.ValueType.ArrayBuilder ? ((ArrayBuilder)x).Append(y) : new ArrayBuilder(x, y);
		}

		public override VerbPresidenceType Presidence => VerbPresidenceType.CreateArray;

	   public override string ToString() => ",";
	}
}