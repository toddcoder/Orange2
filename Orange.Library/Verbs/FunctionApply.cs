using Orange.Library.Values;
using Standard.Types.Maybe;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
	public class FunctionApply : Verb
	{
		const string LOCATION = "Function Apply";

		public override Value Evaluate()
		{
			var stack = State.Stack;
			var y = stack.Pop(true, LOCATION);
			var x = stack.Pop(true, LOCATION);
			Lambda lambda1;
			Lambda lambda2;
			if (x.As<Lambda>().Assign(out lambda1) && y.As<Lambda>().Assign(out lambda2))
				return new FunctionApplication(lambda1, lambda2);
			FunctionApplication application;
			if (x.As<FunctionApplication>().Assign(out application) && y.As<Lambda>().Assign(out lambda2))
			{
				application.Add(lambda2);
				return application;
			}
			return new Nil();
		}

		public override VerbPresidenceType Presidence => VerbPresidenceType.SendMessage;

	   public override string ToString() => ".";
	}
}