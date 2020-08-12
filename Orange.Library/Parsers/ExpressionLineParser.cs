using Orange.Library.Verbs;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;
using static Core.Monads.MonadExtensions;

namespace Orange.Library.Parsers
{
   public class ExpressionLineParser : Parser
   {
      public ExpressionLineParser() : base("^ |tabs|") { }

      public override Verb CreateVerb(string[] tokens)
      {
         if (GetExpression(source, NextPosition, EndOfLine(), true).If(out var block, out var index))
         {
            if (block.Count == 0)
            {
               return null;
            }

            overridePosition = index;
            result.Value = block;
            return new EvaluateExpression(block) { Index = position };
         }

         return null;
      }

      public override string VerboseName => "line expression";
   }
}