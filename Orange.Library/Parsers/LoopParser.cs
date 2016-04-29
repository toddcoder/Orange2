using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.StatementParser;
using static Orange.Library.Parsers.Stop;
using static Orange.Library.Runtime;

namespace Orange.Library.Parsers
{
   public class LoopParser : Parser
   {
      FreeParser parser;

      public LoopParser()
         : base($"^ /(|tabs| 'loop' /s+) /({REGEX_VARIABLE}) /(/s* '=' /s*)")
      {
         parser = new FreeParser();
      }

      public override Verb CreateVerb(string[] tokens)
      {
         var variable = tokens[2];
         Color(position, tokens[1].Length, KeyWords);
         Color(variable.Length, Variables);
         Color(tokens[3].Length, Structures);

         var index = NextPosition;
         Block initialization;
         if (GetExpression(source, index, LoopWhile())
            .Assign(out initialization, out index))
         {
            var builder = new CodeBuilder();
            builder.AssignToNewField(false, variable, initialization);
            var block = builder.Block;
            block.Expression = false;
            initialization = block;
            if (parser.Scan(source, index, "^ /(' '*) /('while' | 'until') /b"))
            {
               var direction = parser.Tokens[2];
               var isWhile = direction == "while";
               parser.Colorize(Whitespaces, KeyWords);
               index = parser.Position;
               Block condition;
               if (GetExpression(source, index, LoopThen()).Assign(out condition, out index))
               {
                  condition.Expression = false;
                  Block increment;
                  if (GetExpression(source, index, LoopEnd())
                     .Assign(out increment, out index))
                  {
                     increment.Expression = false;
                     builder.Clear();
                     builder.Variable(variable);
                     builder.Assign();
                     builder.Inline(increment);
                     increment = builder.Block;
                     Block body;
                     if (GetOneOrMultipleBlock(source, index).Assign(out body, out index))
                     {
                        overridePosition = index;
                        return new Loop(initialization, isWhile, condition, increment, body) { Index = position };
                     }
                  }
               }
            }
         }
         return null;
      }

      public override string VerboseName => "c-for";
   }
}