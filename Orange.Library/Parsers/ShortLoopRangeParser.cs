using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Maybe;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Runtime;
using Block = Orange.Library.Values.Block;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.ShortLambdaParser;
using static Orange.Library.Parsers.Stop;

namespace Orange.Library.Parsers
{
   public class ShortLoopRangeParser : Parser
   {
      FreeParser freeParser;

      public ShortLoopRangeParser()
         : base("^ /(|sp|) /('(' /s* '<-' /s*)")
      {
         freeParser = new FreeParser();
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, tokens[1].Length, Whitespaces);
         Color(tokens[2].Length, Structures);

         var variable = MangledName("0");

         return GetExpression(source, NextPosition, Comma()).Map((init, i) =>
         {
            return GetExpression(source, i, CommaOrCloseParenthesis()).Map((condition, j) =>
            {
               var index = j;
               Block increment;
               if (freeParser.Scan(source, index, "^ |sp| ','"))
               {
                  freeParser.ColorAll(Structures);
                  index = freeParser.Position;
                  var pIncrement = GetExpression(source, index, Comma());
                  if (!pIncrement.Assign(out increment, out index))
                     return null;
                  if (freeParser.Scan(source, index, "^ |sp| ')'"))
                  {
                     freeParser.ColorAll(Structures);
                     index = freeParser.Position;
                  }
                  else
                     return null;
               }
               else
               {
                  if (freeParser.Scan(source, index, "^ |sp| ')'"))
                  {
                     freeParser.ColorAll(Structures);
                     index = freeParser.Position;
                     var builder = new CodeBuilder();
                     builder.Variable(variable);
                     builder.Verb(new Add());
                     builder.Value(1);
                     increment = builder.Block;
                  }
                  else
                     return null;
               }
               overridePosition = index;
               var value = new LoopRange(variable, init, true, condition, increment, new None<Block>());
               result.Value = value;
               return value.PushedVerb;
            }, () => null);
         }, () => null);
      }

      public override string VerboseName => "short loop range";
   }
}