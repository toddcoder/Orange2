using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Objects;
using Standard.Types.Tuples;
using static Orange.Library.Managers.MessageManager;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;
using static Orange.Library.Runtime;

namespace Orange.Library.Parsers
{
	public class SimpleSendMessageParser : Parser
	{
	   BlockOrLambdaParser blockOrLambdaParser;

		public SimpleSendMessageParser(BlockOrLambdaParser blockOrLambdaParser)
			: base($"^ /(/s* '.') /({REGEX_VARIABLE}) /('(')?")
		{
		   this.blockOrLambdaParser = blockOrLambdaParser;
		}

		public override Verb CreateVerb(string[] tokens)
		{
			Color(position, tokens[1].Length, Structures);
			var message = tokens[2];
			Color(message.Length, Messaging);
			Block actualArguments;
			Block executable = null;
			Parameters parameters = null;
			var index = position + length;
			var bracket = tokens[3];
			Lambda lambda = null;
			if (bracket == "(")
			{
				Color(1, Structures);
			   int newIndex;
			   if (GetExpression(source, index, CloseParenthesis()).Assign(out actualArguments, out newIndex))
			      index = newIndex;
			   else
			      actualArguments = new Block();
			}
			else
				actualArguments = new Block();
		   if (blockOrLambdaParser.Scan(source, index))
		   {
            overridePosition = blockOrLambdaParser.Result.Position;
		      return blockOrLambdaParser.Result.Verb.As<IExecutable>().Map(e =>
		      {
		         parameters = e.Parameters;
		         executable = e.Action;
		         MessagingState.RegisterMessageCall(message);
		         var arguments = new Arguments(actualArguments, executable, parameters);
		         if (lambda != null)
		            arguments.AddArgument(lambda);
		         result.Value = new Message(message, arguments);
		         return new NullOp();
		      }, () => new NullOp());
		   }
         return new NullOp();
		}

		public override string VerboseName => "Simple send message";
	}
}