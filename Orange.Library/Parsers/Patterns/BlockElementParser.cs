using Orange.Library.Patterns;
using Orange.Library.Verbs;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.Stop;

namespace Orange.Library.Parsers.Patterns
{
   public class BlockElementParser : Parser, IElementParser
   {
      public BlockElementParser()
         : base("^ /(/s*) '$('")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, tokens[1].Length, Whitespaces);
         Color(1, Structures);
         return GetExpression(source, NextPosition, CloseParenthesis()).Map((block, index) =>
         {
            Element = new BlockElement2(block);
            overridePosition = index;
            return new NullOp();
         }, () => null);
      }

      public override string VerboseName => "block element";

      public Element Element
      {
         get;
         set;
      }
   }
}