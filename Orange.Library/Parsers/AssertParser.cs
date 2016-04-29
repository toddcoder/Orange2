using Orange.Library.Values;
using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;

namespace Orange.Library.Parsers
{
   public class AssertParser : Parser
   {
      StringParser stringParser;
      InterpolatedStringParser interpolatedStringParser;

      public AssertParser()
         : base("^ /(|tabs|) /(('assert' | 'reject') /s+)")
      {
         stringParser = new StringParser();
         interpolatedStringParser = new InterpolatedStringParser();
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, tokens[1].Length, Whitespaces);
         Color(tokens[2].Length, KeyWords);
         var assert = tokens[2].Trim() == "assert";

         return GetExpression(source, NextPosition, FuncThen()).Map((expression, index) =>
         {
            String message;
            if (stringParser.Scan(source, index))
            {
               overridePosition = stringParser.Position;
               message = (String)stringParser.Value;
            }
            else if (interpolatedStringParser.Scan(source, index))
            {
               overridePosition = interpolatedStringParser.Position;
               message = (String)interpolatedStringParser.Value;
            }
            else
               return null;
            return new Assert(assert, expression, message) { Index = position };
         }, () => null);
      }

      public override string VerboseName => "Assert";
   }
}