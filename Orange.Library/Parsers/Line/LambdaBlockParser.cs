using Orange.Library.Parsers.Special;
using Orange.Library.Values;
using Orange.Library.Verbs;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.StatementParser;
using static Orange.Library.Runtime;
using static Standard.Types.Arrays.ArrayFunctions;

namespace Orange.Library.Parsers.Line
{
   public class LambdaBlockParser : Parser
   {
      ParametersParser parameterParser;

      public LambdaBlockParser()
         : base($"^ /(' '* '(') /({REGEX_VARIABLE} | '_' | '(' {REGEX_VARIABLE} (/s* ',' /s* {REGEX_VARIABLE})+ ')') " +
              $"/(/s*) /('->' | '=>') {REGEX_END1}")
      {
         parameterParser = new ParametersParser();
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, tokens[1].Length, Structures);
         var param = tokens[2];
         var type = tokens[4];
         Parameters parameters;
         var index = NextPosition;
         if (param == "_")
         {
            Color(param.Length, Structures);
            parameters = new Parameters();
         }
         else if (param.StartsWith("("))
         {
            var leftLength = tokens[1].Length + 1;
            index = position + leftLength;
            Color(position, leftLength, Structures);
            if (!parameterParser.Parse(source, index).Assign(out parameters, out index))
               return null;
            index = NextPosition;
         }
         else
         {
            Color(param.Length, Variables);
            parameters = new Parameters(array(new Parameter(param)));
         }
         Color(position + tokens[1].Length + tokens[2].Length, tokens[3].Length, Whitespaces);
         Color(type.Length, Structures);
         var splatting = type == "=>";
         return GetBlock(source, index, true).Map((block, i) =>
         {
            index = i;
            var freeParser = new FreeParser();
            if (freeParser.Scan(source, index, $"{Tabs} ')'"))
            {
               freeParser.ColorAll(Structures);
               index = freeParser.Position;
            }
            else
               return null;
            block.Expression = false;
            var lambda = new Lambda(new Region(), block, parameters, false) {Splatting = splatting};
            result.Value = lambda;
            overridePosition = index;
            return new CreateLambda(parameters, block, parameters.Splatting);
         }, () => null);
      }

      public override string VerboseName => "lambda block parser";
   }
}