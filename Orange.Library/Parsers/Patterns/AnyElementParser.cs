using Orange.Library.Patterns;
using Orange.Library.Verbs;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;

namespace Orange.Library.Parsers.Patterns
{
   public class AnyElementParser : Parser, IElementParser
   {
      public AnyElementParser()
         : base("/(^ /s*) '('")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, tokens[1].Length, Whitespaces);
         Color(1, Structures);
         return GetExpression(source, NextPosition, CloseParenthesis()).Map((textBlock, index) =>
         {
            Element = new AnyBlockElement(textBlock, 1);
            overridePosition = index;
            return new NullOp();
         }, () => null);
      }

      public override string VerboseName => "any element";

      public Element Element
      {
         get;
         set;
      }
   }
}