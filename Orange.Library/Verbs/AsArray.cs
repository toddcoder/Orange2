using Orange.Library.Values;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
	public class AsArray : Verb
	{
		public override Value Evaluate()
		{
			var value = Runtime.State.Stack.Pop(true, "As array");
			switch (value.Type)
			{
				case Value.ValueType.String:
					return ((String)value).ToArray();
				case Value.ValueType.Number:
					return ((Double)value).Range();
				case Value.ValueType.Array:
					return ((Array)value).Flatten();
				default:
			      var source = value.As<ISequenceSource>();
					if (source.IsSome)
						return source.Value.Array;
					if (value.IsArray)
						return value.SourceArray;
					return value;
			}
		}

		public override VerbPresidenceType Presidence => VerbPresidenceType.Increment;

	   public override string ToString() => "!";

	   public override int OperandCount => 1;
	}
}