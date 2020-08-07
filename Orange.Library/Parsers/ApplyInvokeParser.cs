using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.Stop;
using static Core.Monads.MonadExtensions;

namespace Orange.Library.Parsers
{
   public class ApplyInvokeParser : Parser
   {
      public ApplyInvokeParser() : base("^ |sp| ':('") { }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, length, Structures);

         if (GetExpression(source, NextPosition, CloseParenthesis()).If(out var block, out var index))
         {
            overridePosition = index;
            var arguments = new Arguments(block);
            return new ApplyInvoke(arguments);
         }

         return null;
      }

      public override string VerboseName => "apply invoke";
   }
}