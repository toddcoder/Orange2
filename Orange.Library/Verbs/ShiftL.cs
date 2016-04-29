using Orange.Library.Values;
using Standard.Types.Maybe;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
	public class ShiftL : Verb
	{
		public override Value Evaluate()
		{
			var stack = State.Stack;
			var source = stack.Pop(true, location(), false);
			var target = stack.Pop(false, location());
			Variable variable;
			var value = target.As<Variable>().Assign(out variable) ? variable.Value : target;
			var result = SendMessage(value, messageName(), source);
			if (variable == null)
				return result;
			variable.Value = result;
			return variable;
		}

	   protected virtual string location() => "SHL";

	   protected virtual string messageName() => "shl";

		public override VerbPresidenceType Presidence => VerbPresidenceType.Apply;

	   public override string ToString() => "<<";
	}
}