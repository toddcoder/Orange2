using Orange.Library.Verbs;
using static Orange.Library.Parsers.ExpressionParser;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.Stop;
using static Core.Monads.MonadExtensions;

namespace Orange.Library.Parsers
{
   public class DeferParser : Parser
   {
      public DeferParser() : base("^ |tabs| 'defer' /b") { }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, length, KeyWords);
         if (GetExpression(source, NextPosition, EndOfLine()).If(out var exp, out var i))
         {
            overridePosition = i;
            return new Defer(exp);
         }

         return null;
      }

      public override string VerboseName => "Defer";
   }
}