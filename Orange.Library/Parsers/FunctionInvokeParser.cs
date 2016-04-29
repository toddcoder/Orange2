using Orange.Library.Verbs;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Runtime;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.Stop;

namespace Orange.Library.Parsers
{
   public class FunctionInvokeParser : Parser
   {
      public FunctionInvokeParser()
         : base($"^ /(|sp|) /({REGEX_VARIABLE}) /('(') /(/s*)")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         var functionName = tokens[2];

         Color(position, tokens[1].Length, Whitespaces);
         Color(functionName.Length, Invokeables);
         Color(tokens[3].Length, Structures);
         Color(tokens[4].Length, Whitespaces);

         return GetExpression(source, NextPosition, CloseParenthesis()).Map((block, index) =>
         {
            var arguments = new Arguments(block);
            var blockOrLambdaParser = new BlockOrLambdaParser();
            if (blockOrLambdaParser.Scan(source, index))
            {
               index = blockOrLambdaParser.Position;
               var value = blockOrLambdaParser.Value;
               arguments.AddArgument(value);
            }
            overridePosition = index;
            return new FunctionInvoke(functionName, arguments);
         }, () => null);
      }

      public override string VerboseName => "function invoke";
   }
}