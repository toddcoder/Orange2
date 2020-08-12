using Core.Monads;
using Orange.Library.Values;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Parser;
using static Orange.Library.Parsers.Stop;
using static Core.Monads.MonadFunctions;

namespace Orange.Library.Parsers
{
   public static class ConditionParser
   {
      static FreeParser parser;

      static ConditionParser() => parser = new FreeParser();

      public static IMaybe<(Block, int)> Parse(string source, int index) => maybe(parser.Scan(source, index, "^ /(/s* 'if') /(/s* '(')"), () =>
      {
         var tokens = parser.Tokens;
         Color(index, tokens[1].Length, KeyWords);
         Color(tokens[2].Length, Structures);
         index = parser.Result.Position;
         return GetExpression(source, index, CloseParenthesis());
      });
   }
}