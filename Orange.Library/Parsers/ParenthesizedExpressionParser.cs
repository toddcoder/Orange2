using Orange.Library.Verbs;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.Stop;

namespace Orange.Library.Parsers
{
   public class ParenthesizedExpressionParser : Parser
   {
      public ParenthesizedExpressionParser()
         : base("^ ' '* '('")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, length, IDEColor.EntityType.Structures);
         return ExpressionParser.GetExpression(source, NextPosition, CloseParenthesis()).Map((block, index) =>
         {
            overridePosition = index;
            return new Push(block);
         }, () =>
         {
            var shortLambdaParser = new ShortLambdaParser("");
            if (shortLambdaParser.Scan(source, NextPosition))
            {
               overridePosition = shortLambdaParser.Position;
               return shortLambdaParser.Verb;
            }
            return null;
         });
      }

      public override string VerboseName => "parenthesized";
   }
}