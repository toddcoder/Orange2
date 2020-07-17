using Orange.Library.Values;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
	public class DefaultToAssign : OperatorAssign
	{
		public override VerbPrecedenceType Precedence => VerbPrecedenceType.Statement;

	   public override Value Execute(Variable variable, Value value) => variable.Value.IsEmpty ? value : variable.Value;

	   public override string Location => "Default to assign";

	   public override string Message => "defaultTo";

	   public override string ToString() => "<>=";
	}
}