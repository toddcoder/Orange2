using Orange.Library.Values;
using Standard.Types.Maybe;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
	public class Is : Verb
	{
		const string LOCATION = "Is";

		public override Value Evaluate()
		{
			var stack = State.Stack;
			var y = stack.Pop(true, LOCATION);
			var x = stack.Pop(true, LOCATION);
			Class builder;
			Object obj;
			if (y.As<Class>().Assign(out builder))
			{
				Assert(x.As<Object>().Assign(out obj), LOCATION, $"{x} isn't an object");
				return obj.Class.IsChildOf(builder);
			}
			Trait trait;
			Assert(y.As<Trait>().Assign(out trait), LOCATION, $"{y} isn't a class or a trait");
			Assert(!x.As<Object>().Assign(out obj), LOCATION, $"{x} isn't an object");
			return obj.Class.ImplementsTrait(trait) || obj.ImplementsInterface(trait);
		}

		public override VerbPresidenceType Presidence => VerbPresidenceType.Apply;

	   public override string ToString() => "is";
	}
}