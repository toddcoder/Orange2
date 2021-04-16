using Core.Assertions;
using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Parsers.ExpressionParser;
using static Core.Monads.MonadExtensions;

namespace Orange.Library.Parsers
{
   public class GuardParser : Parser
   {
      public GuardParser() : base("^ /(|tabs| 'guard' /b)")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, length, KeyWords);

         if (GetExpressionThenBlock(source, NextPosition).If(out var condition, out var block, out var index))
         {
            block.LastIsReturn.Must().BeTrue().OrThrow("Guard", () => "return or stop required");
            overridePosition = index;

            return new Guard(condition, block) { Index = position };
         }

         return null;
      }

      public override string VerboseName => "guard";
   }
}