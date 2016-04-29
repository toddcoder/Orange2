using Orange.Library.Parsers.Special;
using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;
using static Orange.Library.Runtime;

namespace Orange.Library.Parsers
{
   public class MultiAssignParser : Parser
   {
      public MultiAssignParser()
         : base($"^ /(|tabs|) /('val' | 'var' | 'set') /(/s*) /'(' /({REGEX_VARIABLE} (/s* ',' /s* {REGEX_VARIABLE})+ ')')" +
              " /(/s* '=' /s*)")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         var type = tokens[2];
         var readOnly = type == "val";
         var _override = type == "set";
         Color(position, tokens[1].Length, Whitespaces);
         Color(type.Length, KeyWords);
         Color(tokens[3].Length, Whitespaces);
         Color(tokens[4].Length, Structures);
         var parametersParser = new ParametersParser();
         return parametersParser.Parse(source, position + tokens[1].Length + type.Length + tokens[3].Length + 1)
            .Map((parameters, index) => GetExpression(source, NextPosition, EndOfLine()).Map((expression, newIndex) =>
            {
               overridePosition = newIndex;
               return new MultiAssign(parameters, expression, readOnly, _override) { Index = NextPosition };
            }, () => null), () => null);
      }

      public override string VerboseName => "MultiAssign";
   }
}