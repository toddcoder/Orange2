using Orange.Library.Verbs;
using static Orange.Library.Parsers.IDEColor.EntityType;
using static Orange.Library.Runtime;
using Standard.Types.Tuples;
using static Orange.Library.Parsers.ExpressionParser;

namespace Orange.Library.Parsers
{
   public class GuardParser : Parser
   {
      public GuardParser()
         : base($"^ /(|tabs| 'guard' /b)")
      {
      }

      public override Verb CreateVerb(string[] tokens)
      {
         Color(position, length, KeyWords);
         return GetExpressionThenBlock(source, NextPosition).Map((condition, block, index) =>
         {
            Assert(block.LastIsReturn, "Guard", "return required");
            overridePosition = index;
            return new Guard(condition, block) { Index = position };
         }, () => null);
      }

      public override string VerboseName => "guard";
   }
}