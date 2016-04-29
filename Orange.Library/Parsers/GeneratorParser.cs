using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Runtime;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;

namespace Orange.Library.Parsers
{
	public class GeneratorParser : Parser
	{
		public GeneratorParser()
			: base($"^ /(/s*) /'(' /(/s*) /({REGEX_VARIABLE}) /(/s* '<-')")
		{
		}

		public override Verb CreateVerb(string[] tokens)
		{
			Color(position, tokens[1].Length, Whitespaces);
			var delimiter = tokens[2];
			Color(delimiter.Length, Structures);
			Color(tokens[3].Length, Whitespaces);
			var parameterName = tokens[4];
			Color(parameterName.Length, Variables);
			Color(tokens[5].Length, Structures);
		   return GetExpression(source, NextPosition, CloseParenthesis()).Map((block, index) =>
		   {
		      overridePosition = index;
		      var generator = new Generator(parameterName, block);
		      result.Value = generator;
		      return new Push(generator);
		   }, () => null);
		}

		public override string VerboseName => "generator";
	}
}