using Orange.Library.Values;
using Standard.Types.Objects;
using static Orange.Library.Managers.ExpressionManager;
using static Orange.Library.Managers.MessageManager;

namespace Orange.Library.Verbs
{
	public class FlatMap : Verb
	{
		const string LOCATION = "Flat map";
		const string MESSAGE_NAME = "fmap";

		public override Value Evaluate()
		{
			var stack = Runtime.State.Stack;
			var code = stack.Pop(true, LOCATION, false);
			var arrayValue = stack.Pop(true, LOCATION);
			if (arrayValue.IsArray)
			{
				var array = (Array)arrayValue.SourceArray;
			   var lambda = code.As<Lambda>();
				if (lambda.IsSome)
				{
					var arguments = new Arguments(new NullBlock(), lambda.Value.Block, lambda.Value.Parameters);
					return MessagingState.SendMessage(array, MESSAGE_NAME, arguments);
				}
			   var block = code.As<Block>();
				if (block.IsSome)
				{
					var arguments = new Arguments(new NullBlock(), block.Value, new NullParameters());
					return MessagingState.SendMessage(array, MESSAGE_NAME, arguments);
				}
				if (code is IStringify)
				{
					var stringifyBlock = new Block
					{
						new Push(code)
					};
					var arguments = new Arguments(new NullBlock(), stringifyBlock, new NullParameters());
					return MessagingState.SendMessage(array, MESSAGE_NAME, arguments);
				}
			   var format = code.As<Values.Format>();
				if (format.IsSome)
				{
					var formatBlock = new Block
					{
						new Push(format.Value.Stringify.String)
					};
					var arguments = new Arguments(new NullBlock(), formatBlock, format.Value.Parameters);
					return MessagingState.SendMessage(array, MESSAGE_NAME, arguments);
				}
			}
			return arrayValue;
		}

		public override VerbPresidenceType Presidence => VerbPresidenceType.Apply;

	   public override string ToString() => "->&";
	}
}