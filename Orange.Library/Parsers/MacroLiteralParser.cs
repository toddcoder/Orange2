using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;

namespace Orange.Library.Parsers
{
	public class MacroLiteralParser : Parser
	{
		public MacroLiteralParser()
			: base("^ ' '* '.' /(['({'])")
		{
		}

		public override Verb CreateVerb(string[] tokens)
		{
			Color(position, length, Structures);
		   return GetExpression(source, NextPosition, Close()).Map((block, index) =>
		   {
		      result.Value = block;
		      var macro = new Macro();
		      overridePosition = index;
		      return new Push(macro);
		   }, () => null);
		}

		public override string VerboseName => "macro literal";
	}
}