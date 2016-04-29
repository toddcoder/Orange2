using Orange.Library.Values;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
	public class DefaultTo : TwoValueVerb
	{
		public override Value Evaluate(Value x, Value y)
		{
		   var some = x.As<Some>();
		   if (some.IsSome)
		      return some.Value.Value();
		   var none = x.As<None>();
		   if (none.IsSome)
		      return y;
		   return x.IsEmpty ? y : x;
		}

	   public override string Location => "Default to";

	   public override string Message => "defaultTo";

	   public override VerbPresidenceType Presidence => VerbPresidenceType.Or;

	   public override string ToString() => "??";
	}
}