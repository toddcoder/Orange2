using Orange.Library.Values;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
	public class As : Verb
	{
		const string LOCATION = "As";

		public override Value Evaluate()
		{
			var stack = State.Stack;
			var y = stack.Pop(true, LOCATION);
			var x = stack.Pop(true, LOCATION);
		   var cls = y.As<Class>();
			if (cls.IsSome)
			{
			   var obj = x.As<Object>();
			   if (obj.IsSome && obj.Value.Class.IsChildOf(cls.Value))
			      return new Some(obj.Value);
			}
		   var trait = y.As<Trait>();
			if (trait.IsSome)
			{
            var obj = x.As<Object>();
			   if (obj.IsSome && (obj.Value.Class.ImplementsTrait(trait.Value) || obj.Value.ImplementsInterface(trait.Value)))
			      return new Some(obj.Value);
			}
			return new None();
		}

		public override VerbPresidenceType Presidence => VerbPresidenceType.Apply;

	   public override string ToString() => "as";
	}
}