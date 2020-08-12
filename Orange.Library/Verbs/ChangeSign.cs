using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;
using static Orange.Library.Values.Value;

namespace Orange.Library.Verbs
{
	public class ChangeSign : Verb
	{
		const string LOCATION = "Change sign";

		public override Value Evaluate()
		{
			var value = State.Stack.Pop(true, LOCATION);
			if (value.Type == ValueType.Object)
         {
            return SendMessage(value, "neg");
         }

         return -value.Number;
		}

		public override VerbPrecedenceType Precedence => VerbPrecedenceType.ChangeSign;

	   public override string ToString() => "-";

	   public override AffinityType Affinity => AffinityType.Prefix;

	   public override int OperandCount => 1;
	}
}