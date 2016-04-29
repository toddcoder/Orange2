using Orange.Library.Values;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
	public class Then : Verb
	{
		const string LOCATION = "Then";

		public override Value Evaluate()
		{
			var stack = State.Stack;
			var nextValue = stack.Pop(true, LOCATION, false);
		   var executable = nextValue.As<IExecutable>();
			Assert(executable.IsSome, LOCATION, "Must be an executable");
			var seed = stack.Pop(true, LOCATION);
			return new UnboundedGenerator(seed, executable.Value.Action);
		}

		public override VerbPresidenceType Presidence => VerbPresidenceType.Apply;

	   public override string ToString() => "then";
	}
}