using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;
using static Orange.Library.Runtime;
using static Standard.Types.Maybe.Maybe;

namespace Orange.Library.Parsers
{
   public class LoopRangeParser : Parser
   {
      FreeParser parser;

      public LoopRangeParser()
         : base($"^ /(|sp|) /'(' /('loop' /s+) /({REGEX_VARIABLE}) /(/s* '=' /s*)")
      {
         parser = new FreeParser();
      }

      public override Verb CreateVerb(string[] tokens)
      {
         var variable = tokens[4];
         Color(position, tokens[1].Length, Whitespaces);
         Color(tokens[2].Length, Structures);
         Color(tokens[3].Length, KeyWords);
         Color(variable.Length, Variables);
         Color(tokens[5].Length, Structures);

         var index = NextPosition;
         Block init;
         if (GetExpression(source, index, LoopWhile()).Assign(out init, out index))
         {
            if (parser.Scan(source, index, "^ /(' '*) /('while' | 'until') /b"))
            {
               var direction = parser.Tokens[2];
               var positive = direction == "while";
               parser.Colorize(Whitespaces, KeyWords);
               index = parser.Position;
               Block condition;
               if (GetExpression(source, index, LoopThen()).Assign(out condition, out index))
               {
                  condition.Expression = false;
                  Block increment;
                  if (GetExpression(source, index, Yield()).Assign(out increment, out index))
                  {
                     Block yielding;
                     if (GetExpression(source, index, CloseParenthesis()).Assign(out yielding, out index))
                     {
                        overridePosition = index;
                        var someYielding = When(yielding.Count > 0, () => yielding);
                        var value = new LoopRange(variable, init, positive, condition, increment, someYielding);
                        result.Value = value;
                        return value.PushedVerb;
                     }
                  }
               }
            }
         }
         return null;
      }

      public override string VerboseName => "loop range";
   }
}