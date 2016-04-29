using Orange.Library.Verbs;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.Stop;

namespace Orange.Library.Parsers
{
	public class InvokeParser : Parser
	{
		public InvokeParser()
			: base("^ /(':'? '(')")
		{
		}

		public override Verb CreateVerb(string[] tokens)
		{
			Color(length, Structures);

		   return GetExpression(source, NextPosition, CloseParenthesis()).Map((block, index) =>
		   {
		      var arguments = new Arguments(block);
		      overridePosition = index;

		      return new Invoke(arguments);
		   }, () => null);
		}

		public override string VerboseName => "invoke";
	}
}