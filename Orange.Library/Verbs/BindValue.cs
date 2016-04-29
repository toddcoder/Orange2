using Orange.Library.Values;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;

namespace Orange.Library.Verbs
{
	public class BindValue : Verb
	{
		string variableName;
		Value value;

		public BindValue(string variableName, Value value)
		{
			this.variableName = variableName;
			this.value = value;
		}

		public override Value Evaluate()
		{
			Value resolvedValue;
		   var block = value.As<Block>();
			if (block.IsSome)
			{
				block.Value.AutoRegister = false;
				resolvedValue = block.Value.Evaluate().AssignmentValue();
			}
			else
				resolvedValue = value.AssignmentValue();
		   return new BoundValue(variableName, resolvedValue);
		}

		public override VerbPresidenceType Presidence => VerbPresidenceType.Push;

	   public override string ToString() => $"({variableName} := {value}";
	}
}