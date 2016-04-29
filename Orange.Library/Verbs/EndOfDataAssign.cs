using Orange.Library.Values;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
	public class EndOfDataAssign : Verb
	{
		const string LOCATION = "End of data assign";

		public override Value Evaluate()
		{
			var stack = State.Stack;
			var value = stack.Pop(true, LOCATION);
		   var variable = stack.Pop(false, LOCATION).As<Variable>();
			Assert(variable.IsSome, LOCATION, "Expected a variable");
			if (value.Type == Value.ValueType.Nil)
				return false;
			value.AssignmentValue().AssignTo(variable.Value);
			return true;
		}

		public override VerbPresidenceType Presidence => VerbPresidenceType.Statement;

	   public override string ToString() => ":=:";
	}
}