using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
	public class ShiftRightAssign : OperatorAssign
	{
		public override VerbPrecedenceType Precedence => VerbPrecedenceType.Statement;

	   public override Value Execute(Variable variable, Value value) => (int)variable.Number >> (int)value.Number;

	   public override string Location => "Shift right assign";

	   public override string Message => "shright";

	   public override string ToString() => ">>=";
	}
}