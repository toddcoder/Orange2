using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.StatementParser;

namespace Orange.Library.Parsers
{
	public class LazyBlockParser : Parser
	{
		public LazyBlockParser()
			: base("^ /(' '* 'lazy') /b")
		{
		}

		public override Verb CreateVerb(string[] tokens)
		{
			Color(position, tokens[1].Length, KeyWords);
		   return GetOneOrMultipleBlock(source, NextPosition).Map((block, index) =>
		   {
		      overridePosition = index;
		      result.Value = new LazyBlock(block);
		      return new Push(result.Value);
		   }, () => null);
		}

		public override string VerboseName => "Lazy block";
	}
}