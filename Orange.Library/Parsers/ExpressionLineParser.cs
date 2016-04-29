using Orange.Library.Verbs;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;

namespace Orange.Library.Parsers
{
   public class ExpressionLineParser : Parser
   {
      public ExpressionLineParser()
         : base("^ |tabs|")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         return GetExpression(source, NextPosition, EndOfLine(), true).Map((block, index) =>
         {
            overridePosition = index;
            result.Value = block;
            return new EvaluateExpression(block) { Index = NextPosition };
         }, () => null);
      }

      public override string VerboseName => "line expression";
   }
}