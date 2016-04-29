using Orange.Library.Values;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Runtime;

namespace Orange.Library.Verbs
{
	public class Index : Verb
	{
		Arguments arguments;

		public Index(Arguments arguments)
		{
			this.arguments = arguments;
		}

		public override Value Evaluate()
		{
			var value = State.Stack.Pop(false, "Index");
		   var variable = value.As<Variable>();
			if (variable.IsSome)
			{
				var possibleArray = variable.Value.Value;
				if (!possibleArray.IsArray)
					variable.Value.Value = possibleArray.IsEmpty ? new Array() : new Array
					{
						possibleArray
					};
			}
			else
				value = value.Resolve();
			return SendMessage(value, "index", arguments);
		}

		public override VerbPresidenceType Presidence => VerbPresidenceType.Indexer;

	   public override string ToString() => $"<{arguments}>";
	}
}